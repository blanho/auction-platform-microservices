using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace BuildingBlocks.Infrastructure.Locking;

public class EnhancedDistributedLock : IDistributedLock
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<EnhancedDistributedLock> _logger;
    private readonly string _prefix;

    private static readonly TimeSpan DefaultExpiry = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan DefaultWait = TimeSpan.FromSeconds(10);
    private const int MaxRetryAttempts = 5;
    private const int BaseDelayMs = 20;
    private const int MaxDelayMs = 200;

    public EnhancedDistributedLock(
        IConnectionMultiplexer redis,
        ILogger<EnhancedDistributedLock> logger,
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
        var attempt = 0;

        while (DateTime.UtcNow < endTime)
        {
            cancellationToken.ThrowIfCancellationRequested();
            attempt++;

            var acquired = await db.StringSetAsync(
                lockKey,
                lockValue,
                lockExpiry,
                When.NotExists);

            if (acquired)
            {
                _logger.LogDebug("Lock acquired for {ResourceKey} after {Attempts} attempts", resourceKey, attempt);
                return new EnhancedLockHandle(db, lockKey, lockValue, _logger);
            }

            var delay = CalculateBackoffDelay(attempt);
            await Task.Delay(delay, cancellationToken);
        }

        _logger.LogWarning("Failed to acquire lock for {ResourceKey} after {Attempts} attempts", resourceKey, attempt);
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
            return new EnhancedLockHandle(db, lockKey, lockValue, _logger);
        }

        return null;
    }

    public async Task<OptimisticLockResult> AcquireWithRetryAsync(
        string resourceKey,
        Func<CancellationToken, Task<bool>> operation,
        TimeSpan? lockExpiry = null,
        CancellationToken cancellationToken = default)
    {
        var lockKey = $"{_prefix}{resourceKey}";
        var db = _redis.GetDatabase();

        for (int attempt = 1; attempt <= MaxRetryAttempts; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var lockValue = Guid.NewGuid().ToString();
            var acquired = await db.StringSetAsync(
                lockKey,
                lockValue,
                lockExpiry ?? DefaultExpiry,
                When.NotExists);

            if (!acquired)
            {
                var delay = CalculateBackoffDelay(attempt);
                _logger.LogDebug("Lock contention for {ResourceKey}, attempt {Attempt}, backing off {Delay}ms",
                    resourceKey, attempt, delay);
                await Task.Delay(delay, cancellationToken);
                continue;
            }

            try
            {
                var success = await operation(cancellationToken);
                return new OptimisticLockResult(success, attempt, null);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Operation failed for {ResourceKey} on attempt {Attempt}", resourceKey, attempt);
                return new OptimisticLockResult(false, attempt, ex.Message);
            }
            finally
            {
                await ReleaseLockAsync(db, lockKey, lockValue);
            }
        }

        return new OptimisticLockResult(false, MaxRetryAttempts, "Max retry attempts exceeded");
    }

    public async Task ReleaseAsync(string resourceKey, CancellationToken cancellationToken = default)
    {
        var lockKey = $"{_prefix}{resourceKey}";
        var db = _redis.GetDatabase();
        await db.KeyDeleteAsync(lockKey);
    }

    private static int CalculateBackoffDelay(int attempt)
    {

        var delay = BaseDelayMs * (int)Math.Pow(2, attempt - 1);
        delay = Math.Min(delay, MaxDelayMs);

        var jitter = Random.Shared.Next(-delay / 4, delay / 4);
        return delay + jitter;
    }

    private static async Task ReleaseLockAsync(IDatabase db, string lockKey, string lockValue)
    {

        const string script = @"
            if redis.call('get', KEYS[1]) == ARGV[1] then
                return redis.call('del', KEYS[1])
            else
                return 0
            end";

        await db.ScriptEvaluateAsync(script, new RedisKey[] { lockKey }, new RedisValue[] { lockValue });
    }

    private sealed class EnhancedLockHandle : IAsyncDisposable
    {
        private readonly IDatabase _db;
        private readonly string _lockKey;
        private readonly string _lockValue;
        private readonly ILogger _logger;
        private bool _disposed;

        public EnhancedLockHandle(IDatabase db, string lockKey, string lockValue, ILogger logger)
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
                await ReleaseLockAsync(_db, _lockKey, _lockValue);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to release lock {LockKey}", _lockKey);
            }
        }
    }
}

public record OptimisticLockResult(bool Success, int Attempts, string? ErrorMessage);
