using Common.Locking.Abstractions;
using StackExchange.Redis;

namespace Common.Locking.Implementations;

public class RedisDistributedLock : IDistributedLock
{
    private readonly IConnectionMultiplexer _redis;
    private readonly string _prefix;
    private static readonly TimeSpan DefaultExpiry = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan DefaultWait = TimeSpan.FromSeconds(10);

    public RedisDistributedLock(IConnectionMultiplexer redis, string prefix = "lock:")
    {
        _redis = redis;
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

            var acquired = await db.StringSetAsync(
                lockKey,
                lockValue,
                lockExpiry,
                When.NotExists);

            if (acquired)
            {
                return new LockHandle(db, lockKey, lockValue);
            }

            await Task.Delay(50, cancellationToken);
        }

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

        var acquired = await db.StringSetAsync(
            lockKey,
            lockValue,
            lockExpiry,
            When.NotExists);

        if (acquired)
        {
            return new LockHandle(db, lockKey, lockValue);
        }

        return null;
    }

    public async Task ReleaseAsync(string resourceKey, CancellationToken cancellationToken = default)
    {
        var lockKey = $"{_prefix}{resourceKey}";
        var db = _redis.GetDatabase();
        await db.KeyDeleteAsync(lockKey);
    }

    private class LockHandle : IAsyncDisposable
    {
        private readonly IDatabase _db;
        private readonly string _lockKey;
        private readonly string _lockValue;
        private bool _disposed;

        public LockHandle(IDatabase db, string lockKey, string lockValue)
        {
            _db = db;
            _lockKey = lockKey;
            _lockValue = lockValue;
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

            await _db.ScriptEvaluateAsync(
                script,
                new RedisKey[] { _lockKey },
                new RedisValue[] { _lockValue });
        }
    }
}
