// SurePassProvider.cs
using PAN.API.Application.DTOs.Common;
using PAN.API.Application.Mappers;
using PAN.API.Domain.Entities;
using PAN.API.Infrastructure.Logging;
using PAN.API.Infrastructure.Providers.Interfaces;

namespace PAN.API.Infrastructure.Providers.Implementations;

public class SurePassProvider : BaseProvider, ISurePassService
{
    public SurePassProvider(IHttpClientFactory factory)
        : base(factory, "SurepassClient") { }

    public async Task<(PanCommonResponseDto response, string raw)> SurePassVerifyAsync(
        string pan,
        providerpanmaster master,
        string correlationId)
    {
        SafeLogger.App($"[SUREPASS] START | CorrelationId: {correlationId}");

        var raw = await PostAsync(
            baseUrl: master.BaseUrl,
            endpoint: master.Endpoint,
            apiKey: master.ApiKey,
            payload: new { id_number = pan },
            correlationId: correlationId
        );

        var mapped = ProviderMapper.MapSurePass(raw)
                     ?? throw new Exception("SurePass mapping returned null");

        SafeLogger.App($"[SUREPASS] END | PAN: {mapped.Pan}");
        return (mapped, raw);
    }
}