using System.Text;
using System.Text.Json;
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

    public async Task<(bool success, object? response, string providerName)> FallbackAsync(string pan ,string correlationId)
    {
        SafeLogger.App("[START] ProviderFallbackService.ExecuteAsync");

        var masters = _cacheService.GetProviders();
        bool fromCache = masters.Any();

        if (!fromCache)
        {
            SafeLogger.App("Provider Cache MISS — fetching from DB");
            masters = await _masterRepository.GetAllActiveProviders();
            _cacheService.SetProviders(masters);
        }
        else
        {
            SafeLogger.App($"Provider Cache HIT — {masters.Count} providers, no DB call");
        }

        if (!masters.Any())
            throw new AppException("PROVIDER_FAILURE", "No providers configured", 500);

        var orderedProviders = masters
            .Where(x => x.IsActive)
            .OrderBy(x => x.Priority)
            .ToList();

        string lastProvider = orderedProviders.First().ProviderName;

       
        foreach (var master in orderedProviders)
        {
            SafeLogger.App($"Trying: {master.ProviderName}");

            try
            {


                var (response, raw) = master.ProviderName.ToLower() switch
                {
                    "surepass" => await _surePass.SurePassVerifyAsync(pan, master, correlationId),
                    "sprintverify" => await _sprintVerify.SprintVerifyAsync(pan, master, correlationId),
                    _ => throw new Exception($"Unknown provider: {master.ProviderName}")
                };

                response.MasterId = master.Id;
                response.ProviderCacheHit = fromCache;

                lastProvider = master.ProviderName;

                SafeLogger.App($"Provider SUCCESS: {master.ProviderName}");
                return (true, response, lastProvider);
            }
            catch (Exception ex)
            {
                lastProvider = master.ProviderName;
                SafeLogger.Error(ex, $"Provider FAILED: {master.ProviderName}");
              
            }
        }

        SafeLogger.App("All providers FAILED");
        return (false, null, lastProvider);
    }
}