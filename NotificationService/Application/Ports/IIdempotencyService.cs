namespace NotificationService.Application.Ports;

public interface IIdempotencyService
{
    Task<bool> IsProcessedAsync(string key, CancellationToken cancellationToken = default);
    Task MarkAsProcessedAsync(string key, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}
