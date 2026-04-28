using PAN.API.Application.DTOs.Common;
using PAN.API.Domain.Entities;

namespace PAN.API.Infrastructure.Providers.Interfaces;

public interface ISurePassService
{
    Task<(PanCommonResponseDto response, string raw)> SurePassVerifyAsync(
        string pan,
        providerpanmaster master,
        string correlationId
    );
}