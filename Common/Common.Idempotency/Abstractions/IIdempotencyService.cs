namespace Common.Idempotency.Abstractions;

/// <summary>
/// Service for managing idempotent operations using Redis.
/// Prevents duplicate processing of requests with the same idempotency key.
/// </summary>
public interface IIdempotencyService
{
    /// <summary>
    /// Checks if a request with the given key has already been processed.
    /// </summary>
    Task<bool> IsProcessedAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tries to acquire idempotency lock for processing.
    /// Returns false if the request is already being processed or was processed.
    /// </summary>
    Task<IdempotencyResult> TryStartProcessingAsync(
        string key,
        TimeSpan? lockTimeout = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a request as successfully processed and stores the result.
    /// </summary>
    Task<bool> MarkAsProcessedAsync<T>(
        string key,
        T result,
        TimeSpan? ttl = null,
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Marks a request as failed and releases the lock.
    /// </summary>
    Task MarkAsFailedAsync(
        string key,
        string? errorMessage = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the cached result for a previously processed request.
    /// </summary>
    Task<T?> GetResultAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Removes an idempotency entry (useful for testing or manual cleanup).
    /// </summary>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of attempting to start idempotent processing.
/// </summary>
public record IdempotencyResult
{
    public bool CanProcess { get; init; }
    public IdempotencyStatus Status { get; init; }
    public string? CachedResultJson { get; init; }
    public DateTimeOffset? ProcessedAt { get; init; }
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Status of an idempotency key.
/// </summary>
public enum IdempotencyStatus
{
    /// <summary>
    /// The key is new and can be processed.
    /// </summary>
    New,

    /// <summary>
    /// The request is currently being processed by another worker.
    /// </summary>
    Processing,

    /// <summary>
    /// The request was already processed successfully.
    /// </summary>
    Completed,

    /// <summary>
    /// The request previously failed.
    /// </summary>
    Failed
}
