using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace BuildingBlocks.Infrastructure.Locking;

public class RedisLock : IDistributedLock
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisLock> _logger;
    private readonly string _prefix;

    private static readonly TimeSpan DefaultExpiry = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan DefaultWait = TimeSpan.FromSeconds(10);

    public RedisLock(
        IConnectionMultiplexer redis,
        ILogger<RedisLock> logger,
        string prefix = "lock:")
    {
        _redis = redis;
        _logger = logger;
        _prefix = prefix;
    }

    public async Task<IAsyncDisposable?> AcquireAsync(
        string resourceKey,
        TimeSpan? expiry = null,
        TimeSpan? wait = null,
        CancellationToken cancellationToken = default)
    {
        var lockKey = $"{_prefix}{resourceKey}";
        var lockValue = Guid.NewGuid().ToString();
        var lockExpiry = expiry ?? DefaultExpiry;
        var waitTime = wait ?? DefaultWait;
        var endTime = DateTime.UtcNow.Add(waitTime);

        var db = _redis.GetDatabase();

        while (DateTime.UtcNow < endTime)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (await TrySetLockAsync(db, lockKey, lockValue, lockExpiry))
            {
                _logger.LogDebug("Lock acquired for {ResourceKey}", resourceKey);
                return new LockHandle(db, lockKey, lockValue, _logger);
            }

            await Task.Delay(50, cancellationToken);
        }

        _logger.LogWarning("Failed to acquire lock for {ResourceKey} within {Wait}s", resourceKey, waitTime.TotalSeconds);
        return null;
    }

    public async Task<IAsyncDisposable?> TryAcquireAsync(
        string resourceKey,
        TimeSpan? expiry = null,
        CancellationToken cancellationToken = default)
    {
        var lockKey = $"{_prefix}{resourceKey}";
        var lockValue = Guid.NewGuid().ToString();
        var lockExpiry = expiry ?? DefaultExpiry;

        var db = _redis.GetDatabase();

        if (await TrySetLockAsync(db, lockKey, lockValue, lockExpiry))
        {
            return new LockHandle(db, lockKey, lockValue, _logger);
        }

        return null;
    }

    private static async Task<bool> TrySetLockAsync(IDatabase db, string key, string value, TimeSpan expiry)
    {
        return await db.StringSetAsync(key, value, expiry, When.NotExists);
    }

    private sealed class LockHandle : IAsyncDisposable
    {
        private readonly IDatabase _db;
        private readonly string _lockKey;
        private readonly string _lockValue;
        private readonly ILogger _logger;
        private bool _disposed;

        private const string ReleaseScript = @"
            if redis.call('get', KEYS[1]) == ARGV[1] then
                return redis.call('del', KEYS[1])
            else
                return 0
            end";

        public LockHandle(IDatabase db, string lockKey, string lockValue, ILogger logger)
        {
            _db = db;
            _lockKey = lockKey;
            _lockValue = lockValue;
            _logger = logger;
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;
            _disposed = true;

            try
            {
                await _db.ScriptEvaluateAsync(ReleaseScript, new RedisKey[] { _lockKey }, new RedisValue[] { _lockValue });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to release lock {LockKey}", _lockKey);
            }
        }
    }
}
