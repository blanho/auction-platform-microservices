using Common.Caching.Abstractions;

namespace Common.Messaging.Abstractions;

public interface IIdempotencyChecker
{
    Task<bool> IsProcessedAsync(string messageId, CancellationToken cancellationToken = default);
    Task MarkAsProcessedAsync(string messageId, TimeSpan expiry, CancellationToken cancellationToken = default);
}

public class RedisIdempotencyChecker : IIdempotencyChecker
{
    private readonly ICacheService _cache;
    private const string KeyPrefix = "msg:processed:";

    public RedisIdempotencyChecker(ICacheService cache)
    {
        _cache = cache;
    }

    public async Task<bool> IsProcessedAsync(string messageId, CancellationToken cancellationToken = default)
    {
        var key = $"{KeyPrefix}{messageId}";
        var value = await _cache.GetAsync<string>(key, cancellationToken);
        return value != null;
    }

    public async Task MarkAsProcessedAsync(string messageId, TimeSpan expiry, CancellationToken cancellationToken = default)
    {
        var key = $"{KeyPrefix}{messageId}";
        await _cache.SetAsync(key, "1", expiry, cancellationToken);
    }
}
