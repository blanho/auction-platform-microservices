using Common.Caching.Abstractions;
using Microsoft.Extensions.Caching.Distributed;

namespace Common.Caching.Implementations;

public class RedisDistributedLock : IDistributedLock
{
    private readonly IDistributedCache _cache;

    public RedisDistributedLock(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<bool> AcquireLockAsync(string key, TimeSpan expiration, CancellationToken cancellationToken = default)
    {
        var lockKey = $"lock:{key}";
        var existing = await _cache.GetAsync(lockKey, cancellationToken);
        
        if (existing != null)
            return false;

        var lockValue = System.Text.Encoding.UTF8.GetBytes(Guid.NewGuid().ToString());
        await _cache.SetAsync(lockKey, lockValue, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration
        }, cancellationToken);

        return true;
    }

    public async Task<bool> ReleaseLockAsync(string key, CancellationToken cancellationToken = default)
    {
        var lockKey = $"lock:{key}";
        await _cache.RemoveAsync(lockKey, cancellationToken);
        return true;
    }
}
