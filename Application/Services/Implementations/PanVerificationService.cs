using PAN.API.Application.DTOs.Common;
using PAN.API.Application.DTOs.Request;
using PAN.API.Application.Services.Interfaces;
using PAN.API.Domain.Entities;
using PAN.API.Infrastructure.Repositories.Interfaces;
using PAN.API.Utilities;

namespace PAN.API.Application.Services.Implementations;

public class PanVerificationService : IPanVerificationService
{
    private readonly IPanRepository _panRepository;
    private readonly IRawResponseRepository _rawRepository;
    private readonly IFallbackService _fallbackService;
    private readonly ICacheService _cacheService;
    private readonly EncryptionService _encryptionService;

    public PanVerificationService(
        IPanRepository panRepository,
        IRawResponseRepository rawRepository,
        IFallbackService fallbackService,
        ICacheService cacheService,
        EncryptionService encryptionService)
    {
        _panRepository = panRepository;
        _rawRepository = rawRepository;
        _fallbackService = fallbackService;
        _cacheService = cacheService;
        _encryptionService = encryptionService;
    }

    public async Task<PanCommonResponseDto> PanVerifyAsync(
        PanRequest request,
        string correlationId,
        string ip)
    {
        // ✅ NULL / EMPTY CHECK
        if (request == null || string.IsNullOrWhiteSpace(request.IdNumber))
            throw new AppException("INVALID_REQUEST", "PAN is required");

        // ✅ DTO already normalizes value
        var pan = request.IdNumber;

        // ✅ FORMAT VALIDATION
        if (!ValidationHelper.IsValidPan(pan))
            throw new AppException("INVALID_PAN_FORMAT", "Invalid PAN format");

        var hash = HashHelper.ComputeSha256(pan);

        // ─────────────────────────────────────────────
        // ✅ DATABASE CACHE CHECK
        // ─────────────────────────────────────────────
        var existing = await _panRepository.GetByHash(hash);

        if (existing != null)
        {
            return new PanCommonResponseDto
            {
                IsSuccess = true,

                // 🔥 REQUIRED FOR BDD
                Source = "DATABASE",

                // ✅ REAL DECRYPTION
                Pan = _encryptionService.Decrypt(existing.EncryptedPan),
                FullName = _encryptionService.Decrypt(existing.EncryptedFullName),

                PanStatus = existing.PanStatus,
                AadhaarLinked = existing.IsPanAadhaarLinked ?? false,
                Category = existing.PanCardType,

                ProviderName = "database",
                PrimaryProvider = existing.ProviderName?.ToLower(),

                FallbackUsed = false,
                ProviderCacheHit = false
            };
        }

        // ─────────────────────────────────────────────
        // ✅ FALLBACK FLOW
        // ─────────────────────────────────────────────
        var (success, response, providerName) =
            await _fallbackService.FallbackAsync(pan, correlationId);

        if (!success || response == null)
            throw new AppException(
                "PROVIDER_FAILURE",
                "All providers failed",
                502);

        if (response is not PanCommonResponseDto res)
            throw new AppException(
                "MAPPING_FAILED",
                "Invalid provider response",
                500);

        providerName = providerName?.ToLower();

        // ─────────────────────────────────────────────
        // ✅ PROVIDER CONFIG CHECK
        // ─────────────────────────────────────────────
        var providers = _cacheService.GetProviders();

        var providerConfig = providers?
            .FirstOrDefault(p =>
                p.ProviderName.ToLower() == providerName);

        if (providerConfig == null)
            throw new AppException(
                "CONFIG_ERROR",
                "Provider config not found");

        // ─────────────────────────────────────────────
        // ✅ NORMALIZATION
        // ─────────────────────────────────────────────
        res.ProviderName = providerName;
        res.PrimaryProvider ??= providerName;

        // 🔥 REQUIRED FOR BDD
        res.Source = "PROVIDER";

        // ─────────────────────────────────────────────
        // ✅ SAVE TO DATABASE
        // ─────────────────────────────────────────────
        var entity = new PanVerification
        {
            MasterId = res.MasterId,
            ProviderRequestId = res.client_id,

            PanHash = hash,

            EncryptedPan =
                _encryptionService.Encrypt(res.Pan ?? ""),

            EncryptedFullName =
                _encryptionService.Encrypt(res.FullName ?? ""),

            PanStatus = res.PanStatus ?? "101",

            PanLookUpStatus =
                res.IsSuccess ? "SUCCESS" : "FAILED",

            PanCardType = res.Category ?? "person",

            IsPanAadhaarLinked = res.AadhaarLinked,

            CallerIp = ip ?? "",

            CreatedAt = DateTime.UtcNow
        };

        var savedId = await _panRepository.Insert(entity);

        await _rawRepository.InsertAsync(new PanResponseJson
        {
            PanVerificationId = savedId,

            RequestId = res.client_id,

            EncryptedRawResponseJson =
                _encryptionService.Encrypt("{}"),

            CreatedAt = DateTime.UtcNow
        });

        return res;
    }
}