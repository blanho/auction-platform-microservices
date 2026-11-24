using System.Text.Json;
using BuildingBlocks.Application.Abstractions;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace BuildingBlocks.Infrastructure.Caching;

public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly IConnectionMultiplexer? _redis;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public RedisCacheService(IDistributedCache cache, IConnectionMultiplexer? redis = null)
    {
        _cache = cache;
        _redis = redis;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var bytes = await _cache.GetAsync(key, cancellationToken);
        return bytes is { Length: > 0 } ? Deserialize<T>(bytes) : default;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(value, JsonOptions);
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration
        };
        await _cache.SetAsync(key, bytes, options, cancellationToken);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await _cache.RemoveAsync(key, cancellationToken);
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        if (_redis != null)
        {
            return await _redis.GetDatabase().KeyExistsAsync(key);
        }

        var bytes = await _cache.GetAsync(key, cancellationToken);
        return bytes != null && bytes.Length > 0;
    }

    public async Task<T> GetOrSetAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        var bytes = await _cache.GetAsync(key, cancellationToken);
        if (bytes is { Length: > 0 } && Deserialize<T>(bytes) is { } cached)
        {
            return cached;
        }

        var value = await factory(cancellationToken);
        await SetAsync(key, value, expiration, cancellationToken);
        return value;
    }

    private static T? Deserialize<T>(byte[] bytes) => JsonSerializer.Deserialize<T>(bytes, JsonOptions);

    public async Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        if (_redis == null)
        {
            return;
        }

        var db = _redis.GetDatabase();
        var endpoints = _redis.GetEndPoints();
        var keysToDelete = new List<RedisKey>();

        foreach (var endpoint in endpoints)
        {
            var server = _redis.GetServer(endpoint);
            await foreach (var key in server.KeysAsync(pattern: $"{prefix}*", pageSize: 250))
            {
                keysToDelete.Add(key);

                if (keysToDelete.Count >= 1000)
                {
                    await db.KeyDeleteAsync(keysToDelete.ToArray());
                    keysToDelete.Clear();
                }
            }
        }

        if (keysToDelete.Count > 0)
        {
            await db.KeyDeleteAsync(keysToDelete.ToArray());
        }
    }
}
