using Common.Locking.Abstractions;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Common.Locking.Implementations;

public class RedisDistributedLock : IDistributedLock
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisDistributedLock> _logger;

    public RedisDistributedLock(
        IConnectionMultiplexer redis,
        ILogger<RedisDistributedLock> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task<IDistributedLockHandle?> TryAcquireAsync(
        string resourceKey,
        TimeSpan expiry,
        CancellationToken cancellationToken = default)
    {
        var lockKey = $"lock:{resourceKey}";
        var lockValue = Guid.NewGuid().ToString();
        var db = _redis.GetDatabase();

        var acquired = await db.StringSetAsync(
            lockKey,
            lockValue,
            expiry,
            When.NotExists);

        if (acquired)
        {
            _logger.LogDebug("Acquired lock for {ResourceKey}", resourceKey);
            return new RedisLockHandle(db, lockKey, lockValue, _logger);
        }

        _logger.LogDebug("Failed to acquire lock for {ResourceKey}", resourceKey);
        return null;
    }

    public async Task<IDistributedLockHandle> AcquireAsync(
        string resourceKey,
        TimeSpan expiry,
        TimeSpan waitTime,
        TimeSpan retryInterval,
        CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;

        while (DateTime.UtcNow - startTime < waitTime)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var handle = await TryAcquireAsync(resourceKey, expiry, cancellationToken);
            if (handle != null)
            {
                return handle;
            }

            await Task.Delay(retryInterval, cancellationToken);
        }

        throw new TimeoutException($"Failed to acquire lock for {resourceKey} within {waitTime}");
    }

    private class RedisLockHandle : IDistributedLockHandle
    {
        private readonly IDatabase _db;
        private readonly string _lockKey;
        private readonly string _lockValue;
        private readonly ILogger _logger;
        private bool _released;

        public RedisLockHandle(
            IDatabase db,
            string lockKey,
            string lockValue,
            ILogger logger)
        {
            _db = db;
            _lockKey = lockKey;
            _lockValue = lockValue;
            _logger = logger;
            ResourceKey = lockKey.Replace("lock:", "");
            IsAcquired = true;
        }

        public string ResourceKey { get; }
        public bool IsAcquired { get; private set; }

        public async Task<bool> ExtendAsync(TimeSpan extension)
        {
            if (_released) return false;

            var script = @"
                if redis.call('get', KEYS[1]) == ARGV[1] then
                    return redis.call('pexpire', KEYS[1], ARGV[2])
                else
                    return 0
                end";

            var result = await _db.ScriptEvaluateAsync(
                script,
                new RedisKey[] { _lockKey },
                new RedisValue[] { _lockValue, (long)extension.TotalMilliseconds });

            return (long)result! == 1;
        }

        public async Task ReleaseAsync()
        {
            if (_released) return;

            var script = @"
                if redis.call('get', KEYS[1]) == ARGV[1] then
                    return redis.call('del', KEYS[1])
                else
                    return 0
                end";

            await _db.ScriptEvaluateAsync(
                script,
                new RedisKey[] { _lockKey },
                new RedisValue[] { _lockValue });

            _released = true;
            IsAcquired = false;
            _logger.LogDebug("Released lock for {ResourceKey}", ResourceKey);
        }

        public async ValueTask DisposeAsync()
        {
            await ReleaseAsync();
        }
    }
}
