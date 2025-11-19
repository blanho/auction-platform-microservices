namespace Notification.Application.Interfaces;

public interface IIdempotencyService
{

    Task<bool> IsProcessedAsync(string eventId, string channel, CancellationToken ct = default);

    Task MarkAsProcessedAsync(
        string eventId,
        string channel,
        string? messageId = null,
        TimeSpan? ttl = null,
        CancellationToken ct = default);

    Task<IAsyncDisposable?> TryAcquireLockAsync(
        string eventId,
        string channel,
        TimeSpan? lockTimeout = null,
        CancellationToken ct = default);

    Task<bool> IsRateLimitedAsync(
        string userId,
        string channel,
        int windowSeconds = 60,
        int maxCount = 10,
        CancellationToken ct = default);

    Task IncrementRateLimitAsync(
        string userId,
        string channel,
        int windowSeconds = 60,
        CancellationToken ct = default);
}
