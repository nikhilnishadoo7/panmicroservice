// SprintVerifyProvider.cs
using PAN.API.Application.DTOs.Common;
using PAN.API.Application.Mappers;
using PAN.API.Domain.Entities;
using PAN.API.Infrastructure.Logging;
using PAN.API.Infrastructure.Providers.Interfaces;

namespace PAN.API.Infrastructure.Providers.Implementations;

public class SprintVerifyProvider : BaseProvider, ISprintVerifyService
{
    public SprintVerifyProvider(IHttpClientFactory factory)
        : base(factory, "SprintVerifyClient") { }

    public async Task<(PanCommonResponseDto response, string raw)> SprintVerifyAsync(
        string pan,
        providerpanmaster master,
        string correlationId)
    {
        SafeLogger.App($"[SPRINTVERIFY] START | CorrelationId: {correlationId}");

        var raw = await PostAsync(
            baseUrl: master.BaseUrl,
            endpoint: master.Endpoint,
            apiKey: master.ApiKey,
            payload: new { idNumber = pan },
            correlationId: correlationId
        );

        var mapped = ProviderMapper.MapSprint(raw)
                     ?? throw new Exception("SprintVerify mapping returned null");

        SafeLogger.App($"[SPRINTVERIFY] END | PAN: {mapped.Pan}");
        return (mapped, raw);
    }
}