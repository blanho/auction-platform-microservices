using Microsoft.Extensions.Logging;
using Notification.Application.Interfaces;
using StackExchange.Redis;

namespace Notification.Infrastructure.Persistence;

public class RedisIdempotencyService : IIdempotencyService
{
    private readonly IDatabase _db;
    private readonly ILogger<RedisIdempotencyService> _logger;
    private const string KeyPrefix = "notification:idempotency";
    private static readonly TimeSpan DefaultTtl = TimeSpan.FromHours(24);

    public RedisIdempotencyService(
        IConnectionMultiplexer redis,
        ILogger<RedisIdempotencyService> logger)
    {
        _db = redis.GetDatabase();
        _logger = logger;
    }

    public async Task<bool> IsProcessedAsync(string eventId, string channel, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(eventId))
            return false;

        var key = GetIdempotencyKey(eventId, channel);
        return await _db.KeyExistsAsync(key);
    }

    public async Task MarkAsProcessedAsync(
        string eventId,
        string channel,
        string? messageId = null,
        TimeSpan? ttl = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(eventId))
            return;

        var key = GetIdempotencyKey(eventId, channel);
        var value = messageId ?? DateTimeOffset.UtcNow.ToString("O");
        await _db.StringSetAsync(key, value, ttl ?? DefaultTtl);
    }

    public async Task<IAsyncDisposable?> TryAcquireLockAsync(
        string eventId,
        string channel,
        TimeSpan? lockTimeout = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(eventId))
            return null;

        var key = GetIdempotencyKey(eventId, channel);
        var wasSet = await _db.StringSetAsync(key, "processing", lockTimeout ?? TimeSpan.FromMinutes(5), When.NotExists);
        
        return wasSet ? new NoOpDisposable() : null;
    }

    public Task<bool> IsRateLimitedAsync(
        string userId,
        string channel,
        int windowSeconds = 60,
        int maxCount = 10,
        CancellationToken ct = default)
    {
        return Task.FromResult(false);
    }

    public Task IncrementRateLimitAsync(
        string userId,
        string channel,
        int windowSeconds = 60,
        CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }

    private static string GetIdempotencyKey(string eventId, string channel)
        => $"{KeyPrefix}:{eventId}:{channel.ToLowerInvariant()}";

    private sealed class NoOpDisposable : IAsyncDisposable
    {
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}