// Infrastructure/Providers/Interfaces/ISprintVerifyService.cs
using PAN.API.Application.DTOs.Common;
using PAN.API.Domain.Entities;

namespace PAN.API.Infrastructure.Providers.Interfaces;

public interface ISprintVerifyService
{
    Task<(PanCommonResponseDto response, string raw)> SprintVerifyAsync(
        string pan,
        providerpanmaster master,
        string correlationId
    );
}