namespace BuildingBlocks.Infrastructure.Locking;

public interface IDistributedLock
{
    Task<IAsyncDisposable?> AcquireAsync(
        string resourceKey, 
        TimeSpan? expiry = null, 
        TimeSpan? wait = null, 
        CancellationToken cancellationToken = default);
    
    Task<IAsyncDisposable?> TryAcquireAsync(
        string resourceKey, 
        TimeSpan? expiry = null, 
        CancellationToken cancellationToken = default);
}
