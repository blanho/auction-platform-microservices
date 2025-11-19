using Microsoft.Extensions.Logging;
using Notification.Application.Interfaces;
using StackExchange.Redis;

namespace Notification.Infrastructure.Services;

public class RedisIdempotencyService : IIdempotencyService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisIdempotencyService> _logger;

    private const string KeyPrefix = "notification:idempotency";
    private const string LockPrefix = "notification:lock";
    private const string RateLimitPrefix = "notification:ratelimit";
    private static readonly TimeSpan DefaultTtl = TimeSpan.FromHours(24);
    private static readonly TimeSpan DefaultLockTimeout = TimeSpan.FromMinutes(5);

    public RedisIdempotencyService(
        IConnectionMultiplexer redis,
        ILogger<RedisIdempotencyService> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task<bool> IsProcessedAsync(string eventId, string channel, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(eventId))
            return false;

        var db = _redis.GetDatabase();
        var key = GetIdempotencyKey(eventId, channel);

        var exists = await db.KeyExistsAsync(key);

        if (exists)
        {
            _logger.LogDebug(
                "Notification already processed: EventId={EventId}, Channel={Channel}",
                eventId, channel);
        }

        return exists;
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

        var db = _redis.GetDatabase();
        var key = GetIdempotencyKey(eventId, channel);
        var value = new IdempotencyValue
        {
            MessageId = messageId,
            ProcessedAt = DateTimeOffset.UtcNow
        };

        var expiry = ttl ?? DefaultTtl;

        await db.StringSetAsync(
            key,
            System.Text.Json.JsonSerializer.Serialize(value),
            expiry);

        _logger.LogDebug(
            "Marked notification as processed: EventId={EventId}, Channel={Channel}, TTL={TTL}",
            eventId, channel, expiry);
    }

    public async Task<IAsyncDisposable?> TryAcquireLockAsync(
        string eventId,
        string channel,
        TimeSpan? lockTimeout = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(eventId))
            return null;

        var db = _redis.GetDatabase();
        var key = GetLockKey(eventId, channel);
        var lockValue = Guid.NewGuid().ToString();
        var timeout = lockTimeout ?? DefaultLockTimeout;

        var acquired = await db.StringSetAsync(
            key,
            lockValue,
            timeout,
            When.NotExists);

        if (acquired)
        {
            _logger.LogDebug(
                "Acquired processing lock: EventId={EventId}, Channel={Channel}",
                eventId, channel);

            return new RedisLock(db, key, lockValue, _logger);
        }

        _logger.LogDebug(
            "Failed to acquire lock (already held): EventId={EventId}, Channel={Channel}",
            eventId, channel);

        return null;
    }

    public async Task<bool> IsRateLimitedAsync(
        string userId,
        string channel,
        int windowSeconds = 60,
        int maxCount = 10,
        CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(userId))
            return false;

        var db = _redis.GetDatabase();
        var key = GetRateLimitKey(userId, channel, windowSeconds);

        var count = await db.StringGetAsync(key);

        if (count.HasValue && (int)count >= maxCount)
        {
            _logger.LogWarning(
                "Rate limit exceeded: UserId={UserId}, Channel={Channel}, Count={Count}/{Max}",
                userId, channel, (int)count, maxCount);
            return true;
        }

        return false;
    }

    public async Task IncrementRateLimitAsync(
        string userId,
        string channel,
        int windowSeconds = 60,
        CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(userId))
            return;

        var db = _redis.GetDatabase();
        var key = GetRateLimitKey(userId, channel, windowSeconds);

        var transaction = db.CreateTransaction();
        _ = transaction.StringIncrementAsync(key);
        _ = transaction.KeyExpireAsync(key, TimeSpan.FromSeconds(windowSeconds), ExpireWhen.HasNoExpiry);

        await transaction.ExecuteAsync();
    }

    private static string GetIdempotencyKey(string eventId, string channel)
        => $"{KeyPrefix}:{eventId}:{channel.ToLowerInvariant()}";

    private static string GetLockKey(string eventId, string channel)
        => $"{LockPrefix}:{eventId}:{channel.ToLowerInvariant()}";

    private static string GetRateLimitKey(string userId, string channel, int windowSeconds)
        => $"{RateLimitPrefix}:{userId}:{channel.ToLowerInvariant()}:{DateTimeOffset.UtcNow.ToUnixTimeSeconds() / windowSeconds}";

    private sealed class IdempotencyValue
    {
        public string? MessageId { get; set; }
        public DateTimeOffset ProcessedAt { get; set; }
    }

    private sealed class RedisLock : IAsyncDisposable
    {
        private readonly IDatabase _db;
        private readonly string _key;
        private readonly string _value;
        private readonly ILogger _logger;
        private bool _disposed;

        public RedisLock(IDatabase db, string key, string value, ILogger logger)
        {
            _db = db;
            _key = key;
            _value = value;
            _logger = logger;
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;
            _disposed = true;

            var script = @"
                if redis.call('get', KEYS[1]) == ARGV[1] then
                    return redis.call('del', KEYS[1])
                else
                    return 0
                end";

            try
            {
                await _db.ScriptEvaluateAsync(script, new RedisKey[] { _key }, new RedisValue[] { _value });
                _logger.LogDebug("Released processing lock: Key={Key}", _key);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to release lock: Key={Key}", _key);
            }
        }
    }
}
