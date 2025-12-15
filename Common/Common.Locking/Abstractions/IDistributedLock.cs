namespace Common.Locking.Abstractions;

public interface IDistributedLock
{
    Task<IDistributedLockHandle?> TryAcquireAsync(
        string resourceKey,
        TimeSpan expiry,
        CancellationToken cancellationToken = default);

    Task<IDistributedLockHandle> AcquireAsync(
        string resourceKey,
        TimeSpan expiry,
        TimeSpan waitTime,
        TimeSpan retryInterval,
        CancellationToken cancellationToken = default);
}

public interface IDistributedLockHandle : IAsyncDisposable
{
    string ResourceKey { get; }
    bool IsAcquired { get; }
    Task<bool> ExtendAsync(TimeSpan extension);
    Task ReleaseAsync();
}
