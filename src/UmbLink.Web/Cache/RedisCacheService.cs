using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using UmbLink.Application.Interfaces;

namespace UmbLink.Web.Cache;

public class RedisCacheService(IDistributedCache cache) : ICacheService
{
    static readonly JsonSerializerOptions _jsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        var data = await cache.GetStringAsync(key);
        return data is null ? null : JsonSerializer.Deserialize<T>(data, _jsonOpts);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class
    {
        var json = JsonSerializer.Serialize(value, _jsonOpts);
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiry ?? TimeSpan.FromMinutes(5)
        };
        await cache.SetStringAsync(key, json, options);
    }

    public async Task RemoveAsync(string key)
    {
        await cache.RemoveAsync(key);
    }

    public Task RemoveByPrefixAsync(string prefix)
    {
        // DistributedCache doesn't support prefix deletion natively.
        // With Redis directly, we'd use SCAN + DEL.
        // Callers should use specific key removal instead.
        return Task.CompletedTask;
    }
}
