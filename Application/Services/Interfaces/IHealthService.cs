using PAN.API.Application.DTOs.Response;

namespace PAN.API.Application.Services.Interfaces;

public interface IHealthService
{
    Task<HealthResponse> GetHealthAsync();
    Task<HealthReadyResponse> GetHealthReadyAsync();
}