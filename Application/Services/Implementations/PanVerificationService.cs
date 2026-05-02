using Newtonsoft.Json;
using PAN.API.Application.DTOs.Common;
using PAN.API.Application.DTOs.Request;
using PAN.API.Application.Services.Interfaces;
using PAN.API.Domain.Entities;
using PAN.API.Infrastructure.Logging;
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
        SafeLogger.App($"[START] PanVerifyAsync | CorrelationId: {correlationId}");

        var pan = request.IdNumber;

        if (string.IsNullOrWhiteSpace(pan))
            throw new AppException("INVALID_REQUEST", "PAN is required");

        var hash = HashHelper.ComputeSha256(pan);
        SafeLogger.App($"Hash: {hash}");

        
        var existing = await _panRepository.GetByHash(hash);

        if (existing != null && !string.IsNullOrEmpty(existing.PanHash))
        {
            SafeLogger.App("PAN Cache HIT — returning from DB");

            return new PanCommonResponseDto
            {
                IsSuccess = existing.PanLookUpStatus == "SUCCESS",
                Pan = _encryptionService.Decrypt(existing.EncryptedPan),
                FullName = _encryptionService.Decrypt(existing.EncryptedFullName),
                PanStatus = existing.PanStatus,
                AadhaarLinked = existing.IsPanAadhaarLinked ?? false,
                Category = existing.PanCardType,
                client_id = existing.ProviderRequestId,
                ProviderName = "DATABASE",
                PrimaryProvider = existing.ProviderName,
                Status = existing.PanLookUpStatus ?? "SUCCESS",
                Code = existing.PanStatus ?? "VALID",
                Message = "Fetched from Database",
                FallbackUsed = false,
                ProviderCacheHit = false
            };
        }

        SafeLogger.App("PAN Cache MISS — calling providers");

        
        var (success, response, providerName) = await _fallbackService.FallbackAsync(pan, correlationId);

        if (!success || response == null)
            throw new AppException("PROVIDER_FAILURE", "All PAN providers failed", 502);

        if (response is not PanCommonResponseDto res)
            throw new AppException("MAPPING_FAILED", "Provider response mapping failed", 500);

        res.Source = "PROVIDER";
    

        SafeLogger.App($"Provider SUCCESS: {providerName}");

        SafeLogger.App("MAPPED RESPONSE DATA", new
        {
            MasterId = res.MasterId,
            Pan = res.Pan,
            FullName = res.FullName
        });

        //SafeLogger.App($"Inserting panverification: {entity.Id}");

        var entity = new PanVerification
        {
            Id = Guid.NewGuid(),
            MasterId = res.MasterId,
            ProviderRequestId = res.client_id,
            PanHash = hash,
            EncryptedPan = _encryptionService.Encrypt(res.Pan ?? string.Empty),
            EncryptedFullName = _encryptionService.Encrypt(res.FullName ?? string.Empty),
            PanStatus = res.PanStatus ?? "101",
            PanLookUpStatus = res.IsSuccess ? "SUCCESS" : "FAILED",
            PanCardType = res.Category ?? "person",
            IsPanAadhaarLinked = res.AadhaarLinked,
            CorrelationId = correlationId ?? "",
            CallerIp = ip ?? "",
            CreatedAt = DateTime.UtcNow
        };
        SafeLogger.App("MAPPED RESPONSE DATA", new
        {
            MasterId = res.MasterId,
            RequestId = res.client_id,
            Pan = res.Pan,
            FullName = res.FullName
        });

        SafeLogger.App($"Inserting panverification: {entity.Id}");

        
        var savedId = await _panRepository.Insert(entity);

        
        var rawEntity = new PanResponseJson
        {
            CorrelationId = correlationId ?? "",
            PanVerificationId = savedId,             
            RequestId = res.client_id,
            EncryptedRawResponseJson = _encryptionService.Encrypt(
                                           JsonConvert.SerializeObject(response)),
            CreatedAt = DateTime.UtcNow
        };

        SafeLogger.App("Inserting panresponsesjson");
        await _rawRepository.InsertAsync(rawEntity);

        SafeLogger.App("[END] VerifyAsync SUCCESS");
        return res;
    }
}