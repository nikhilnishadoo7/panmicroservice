using PAN.API.Application.DTOs.Common;
using PAN.API.Application.Services.Interfaces;
using PAN.API.Domain.Entities;
using PAN.API.Infrastructure.Logging;
using PAN.API.Infrastructure.Providers.Interfaces;
using PAN.API.Infrastructure.Repositories.Interfaces;
using PAN.API.Utilities;

namespace PAN.API.Application.Services.Implementations;

public class ProviderFallbackService : IFallbackService
{
    private readonly ISurePassService _surePass;
    private readonly ISprintVerifyService _sprintVerify;
    private readonly IMasterRepository _masterRepository;
    private readonly ICacheService _cacheService;

    public ProviderFallbackService(
        ISurePassService surePass,
        ISprintVerifyService sprintVerify,
        IMasterRepository masterRepository,
        ICacheService cacheService)
    {
        _surePass = surePass;
        _sprintVerify = sprintVerify;
        _masterRepository = masterRepository;
        _cacheService = cacheService;
    }

    public async Task<(bool success, object? response, string providerName)> FallbackAsync(
        string pan,
        string correlationId)
    {
        SafeLogger.App("[START] ProviderFallbackService.FallbackAsync");

        var providers = _cacheService.GetProviders() ?? new List<providerpanmaster>();
        bool fromCache = providers.Any();

        // 🔹 Cache miss → DB
        if (!fromCache)
        {
            SafeLogger.App("Cache MISS → Fetching providers from DB");

            providers = await _masterRepository.GetAllActiveProviders();

            if (providers == null || !providers.Any())
                throw new AppException("PROVIDER_FAILURE", "No providers configured", 500);

            _cacheService.SetProviders(providers);
        }

        // 🔹 Order providers
        var orderedProviders = providers
            .Where(x => x.IsActive)
            .OrderBy(x => x.Priority)
            .ToList();

        if (!orderedProviders.Any())
            throw new AppException("PROVIDER_FAILURE", "No active providers", 500);

        string primaryProvider = orderedProviders.First().ProviderName.ToLower();
        string lastProvider = primaryProvider;

        bool isFirst = true;

        // 🔹 Try providers
        foreach (var master in orderedProviders)
        {
            try
            {
                var providerName = master.ProviderName.ToLower();

                var (response, raw) = providerName switch
                {
                    "surepass" => await _surePass.SurePassVerifyAsync(pan, master, correlationId),
                    "sprintverify" => await _sprintVerify.SprintVerifyAsync(pan, master, correlationId),
                    _ => throw new Exception($"Unknown provider: {providerName}")
                };

                if (response == null || !response.IsSuccess)
                {
                    isFirst = false;
                    continue;
                }

                // ✅ SUCCESS RESPONSE ENRICHMENT
                response.MasterId = master.Id;
                response.ProviderCacheHit = fromCache;
                response.FallbackUsed = !isFirst;
                response.PrimaryProvider = primaryProvider;
                response.ProviderName = providerName;

                return (true, response, providerName);
            }
            catch (Exception ex)
            {
                SafeLogger.Error(ex, $"FAILED → {master.ProviderName}");
            }

            isFirst = false;
            lastProvider = master.ProviderName.ToLower();
        }

        SafeLogger.App("All providers FAILED");

        return (false, null, lastProvider);
    }
}