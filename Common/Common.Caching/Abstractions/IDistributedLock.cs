namespace Common.Caching.Abstractions;

public interface IDistributedLock
{
    Task<bool> AcquireLockAsync(string key, TimeSpan expiration, CancellationToken cancellationToken = default);
    Task<bool> ReleaseLockAsync(string key, CancellationToken cancellationToken = default);
}
