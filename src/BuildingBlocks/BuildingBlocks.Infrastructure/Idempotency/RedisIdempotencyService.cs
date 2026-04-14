using BuildingBlocks.Application.Abstractions;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace BuildingBlocks.Infrastructure.Idempotency;

public sealed class RedisIdempotencyService : IIdempotencyService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisIdempotencyService> _logger;
    private static readonly TimeSpan DefaultExpiry = TimeSpan.FromHours(24);
    private const string KeyPrefix = "idempotency:";

    public RedisIdempotencyService(
        IConnectionMultiplexer redis,
        ILogger<RedisIdempotencyService> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task<bool> IsProcessedAsync(string idempotencyKey, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        return await db.KeyExistsAsync($"{KeyPrefix}{idempotencyKey}");
    }

    public async Task MarkAsProcessedAsync(string idempotencyKey, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var effectiveExpiry = expiry ?? DefaultExpiry;
        await db.StringSetAsync($"{KeyPrefix}{idempotencyKey}", "1", effectiveExpiry);
        _logger.LogDebug("Marked {IdempotencyKey} as processed with expiry {Expiry}", idempotencyKey, effectiveExpiry);
    }
}
