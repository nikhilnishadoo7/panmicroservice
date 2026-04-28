using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PAN.API.Application.Services.Interfaces;
using PAN.API.Infrastructure.Logging;
using PAN.API.Infrastructure.Repositories.Interfaces;

namespace PAN.API.Application.Services.Implementations;

public class CacheWarmupService : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public CacheWarmupService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        SafeLogger.App("CacheWarmup STARTED");

        using var scope = _scopeFactory.CreateScope();

        var masterRepo = scope.ServiceProvider.GetRequiredService<IMasterRepository>();
        var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();

        var providers = await masterRepo.GetAllActiveProviders();

        cacheService.SetProviders(providers);

        SafeLogger.App($"CacheWarmup DONE | Loaded {providers.Count} providers");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        SafeLogger.App("CacheWarmup STOPPED");
        return Task.CompletedTask;
    }
}