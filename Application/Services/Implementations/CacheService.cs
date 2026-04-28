// Application/Services/Implementations/CacheService.cs
using Microsoft.Extensions.Caching.Memory;
using PAN.API.Application.Services.Interfaces;
using PAN.API.Domain.Entities;
using PAN.API.Infrastructure.Logging;

namespace PAN.API.Application.Services.Implementations;

public class CacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private const string ProvidersKey = "providers:master";

    public CacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public List<providerpanmaster> GetProviders()
    {
        _cache.TryGetValue(ProvidersKey, out List<providerpanmaster>? providers);
        return providers ?? new List<providerpanmaster>();
    }

    public void SetProviders(List<providerpanmaster> providers)
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24),
            SlidingExpiration = TimeSpan.FromHours(6)
        };

        _cache.Set(ProvidersKey, providers, options);
        SafeLogger.App($"Cache SET | Providers count: {providers.Count}");
    }

    public void InvalidateProviders()
    {
        _cache.Remove(ProvidersKey);
        SafeLogger.App("Cache INVALIDATED | Providers");
    }
}