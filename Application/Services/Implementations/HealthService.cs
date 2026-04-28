using PAN.API.Application.DTOs.Response;
using PAN.API.Application.Services.Interfaces;
using PAN.API.Infrastructure.Logging;
using PAN.API.Infrastructure.Repositories.Interfaces;

namespace PAN.API.Application.Services.Implementations;

public class HealthService : IHealthService
{
    private readonly IHealthRepository _healthRepository;

    public HealthService(IHealthRepository healthRepository)
    {
        _healthRepository = healthRepository;
    }

    public Task<HealthResponse> GetHealthAsync()
    {
        SafeLogger.App("HealthService: Liveness check");

        return Task.FromResult(new HealthResponse
        {
            Status = "Healthy",
            Service = "PanVerificationService",
            Version = "v0.1",
            Timestamp = DateTime.UtcNow
        });
    }

    public async Task<HealthReadyResponse> GetHealthReadyAsync()
    {
        SafeLogger.App("HealthService: Readiness check");

        var dbHealthy = await _healthRepository.IsDatabaseHealthyAsync();

        if (dbHealthy)
        {
            return new HealthReadyResponse
            {
                Status = "Healthy",
                Checks = new
                {
                    database = "Connected"
                },
                Service = "PanVerificationService",
                Timestamp = DateTime.UtcNow
            };
        }

        return new HealthReadyResponse
        {
            Status = "Unhealthy",
            Checks = new
            {
                database = "Failed"
            },
            Error = "Database connection failed",
            Service = "PanVerificationService",
            Timestamp = DateTime.UtcNow
        };
    }
}