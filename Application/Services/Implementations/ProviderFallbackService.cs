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

    public async Task<(bool success, object? response, string providerName)> FallbackAsync(
        string pan,
        string correlationId)
    {
        var providers = _cacheService.GetProviders();

        if (!providers.Any())
            providers = await _masterRepository.GetAllActiveProviders();

        var ordered = providers
            .Where(x => x.IsActive)
            .OrderBy(x => x.Priority)
            .ToList();

        foreach (var master in ordered)
        {
            try
            {
                SafeLogger.App($"Trying: {master.ProviderName}");

                var result = master.ProviderName.ToLower() switch
                {
                    "surepass" => await _surePass.SurePassVerifyAsync(pan, master, correlationId),
                    "sprintverify" => await _sprintVerify.SprintVerifyAsync(pan, master, correlationId),
                    _ => throw new Exception("Unknown provider")
                };
                result.response.MasterId = master.Id;
                return (true, result.response, master.ProviderName);
            }
            catch (Exception ex)
            {
                SafeLogger.Error(ex, $"FAILED: {master.ProviderName}");
            }
        }

        return (false, null, "NONE");
    }
}