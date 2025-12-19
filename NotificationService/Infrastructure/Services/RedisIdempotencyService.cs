using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using NotificationService.Application.Ports;

namespace NotificationService.Infrastructure.Services;

public class RedisIdempotencyService : IIdempotencyService
{
    private readonly IDistributedCache _cache;
    private const string KeyPrefix = "notification:idempotency:";

    public RedisIdempotencyService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<bool> IsProcessedAsync(string key, CancellationToken cancellationToken = default)
    {
        var cacheKey = KeyPrefix + key;
        var value = await _cache.GetStringAsync(cacheKey, cancellationToken);
        return value != null;
    }

    public async Task MarkAsProcessedAsync(
        string key,
        TimeSpan? ttl = null,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = KeyPrefix + key;
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl ?? TimeSpan.FromHours(24)
        };

        var value = JsonSerializer.Serialize(new IdempotencyRecord
        {
            ProcessedAt = DateTimeOffset.UtcNow,
            Key = key
        });

        await _cache.SetStringAsync(cacheKey, value, options, cancellationToken);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        var cacheKey = KeyPrefix + key;
        await _cache.RemoveAsync(cacheKey, cancellationToken);
    }

    private record IdempotencyRecord
    {
        public DateTimeOffset ProcessedAt { get; init; }
        public string Key { get; init; } = string.Empty;
    }
}
