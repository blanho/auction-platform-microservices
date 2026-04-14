namespace BuildingBlocks.Application.Abstractions;

public interface IMessageDeduplicationService
{
    Task<bool> IsProcessedAsync(string idempotencyKey, CancellationToken cancellationToken = default);
    Task MarkAsProcessedAsync(string idempotencyKey, TimeSpan? expiry = null, CancellationToken cancellationToken = default);
}
