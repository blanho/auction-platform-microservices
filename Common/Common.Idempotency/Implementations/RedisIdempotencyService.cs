using System.Text.Json;
using Common.Idempotency.Abstractions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Common.Idempotency.Implementations;

/// <summary>
/// Redis-based idempotency service with distributed locking support.
/// </summary>
public class RedisIdempotencyService : IIdempotencyService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<RedisIdempotencyService> _logger;
    private readonly IdempotencyOptions _options;
    
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public RedisIdempotencyService(
        IDistributedCache cache,
        ILogger<RedisIdempotencyService> logger,
        IdempotencyOptions? options = null)
    {
        _cache = cache;
        _logger = logger;
        _options = options ?? new IdempotencyOptions();
    }

    public async Task<bool> IsProcessedAsync(string key, CancellationToken cancellationToken = default)
    {
        var cacheKey = GetCacheKey(key);
        var value = await _cache.GetStringAsync(cacheKey, cancellationToken);
        
        if (value == null)
            return false;

        var record = JsonSerializer.Deserialize<IdempotencyRecord>(value, JsonOptions);
        return record?.Status == IdempotencyStatus.Completed;
    }

    public async Task<IdempotencyResult> TryStartProcessingAsync(
        string key,
        TimeSpan? lockTimeout = null,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = GetCacheKey(key);
        var existingValue = await _cache.GetStringAsync(cacheKey, cancellationToken);

        if (existingValue != null)
        {
            var existingRecord = JsonSerializer.Deserialize<IdempotencyRecord>(existingValue, JsonOptions);
            
            if (existingRecord != null)
            {
                // Check if it's a stale processing lock
                if (existingRecord.Status == IdempotencyStatus.Processing)
                {
                    var processingTimeout = lockTimeout ?? _options.DefaultLockTimeout;
                    if (existingRecord.StartedAt.Add(processingTimeout) < DateTimeOffset.UtcNow)
                    {
                        _logger.LogWarning(
                            "Stale idempotency lock detected for key {Key}. Allowing new processing.",
                            key);
                    }
                    else
                    {
                        return new IdempotencyResult
                        {
                            CanProcess = false,
                            Status = IdempotencyStatus.Processing,
                            ErrorMessage = "Request is currently being processed"
                        };
                    }
                }
                else if (existingRecord.Status == IdempotencyStatus.Completed)
                {
                    return new IdempotencyResult
                    {
                        CanProcess = false,
                        Status = IdempotencyStatus.Completed,
                        CachedResultJson = existingRecord.ResultJson,
                        ProcessedAt = existingRecord.CompletedAt
                    };
                }
                else if (existingRecord.Status == IdempotencyStatus.Failed && !_options.AllowRetryOnFailure)
                {
                    return new IdempotencyResult
                    {
                        CanProcess = false,
                        Status = IdempotencyStatus.Failed,
                        ErrorMessage = existingRecord.ErrorMessage
                    };
                }
            }
        }

        // Try to acquire the lock
        var processingRecord = new IdempotencyRecord
        {
            Key = key,
            Status = IdempotencyStatus.Processing,
            StartedAt = DateTimeOffset.UtcNow
        };

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = lockTimeout ?? _options.DefaultLockTimeout
        };

        try
        {
            await _cache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(processingRecord, JsonOptions),
                options,
                cancellationToken);

            _logger.LogDebug("Acquired idempotency lock for key {Key}", key);

            return new IdempotencyResult
            {
                CanProcess = true,
                Status = IdempotencyStatus.New
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to acquire idempotency lock for key {Key}", key);
            return new IdempotencyResult
            {
                CanProcess = false,
                Status = IdempotencyStatus.Processing,
                ErrorMessage = "Failed to acquire processing lock"
            };
        }
    }

    public async Task<bool> MarkAsProcessedAsync<T>(
        string key,
        T result,
        TimeSpan? ttl = null,
        CancellationToken cancellationToken = default) where T : class
    {
        var cacheKey = GetCacheKey(key);

        var record = new IdempotencyRecord
        {
            Key = key,
            Status = IdempotencyStatus.Completed,
            StartedAt = DateTimeOffset.UtcNow,
            CompletedAt = DateTimeOffset.UtcNow,
            ResultJson = JsonSerializer.Serialize(result, JsonOptions)
        };

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl ?? _options.DefaultResultTtl
        };

        try
        {
            await _cache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(record, JsonOptions),
                options,
                cancellationToken);

            _logger.LogDebug("Marked idempotency key {Key} as completed", key);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark idempotency key {Key} as completed", key);
            return false;
        }
    }

    public async Task MarkAsFailedAsync(
        string key,
        string? errorMessage = null,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = GetCacheKey(key);

        if (_options.AllowRetryOnFailure)
        {
            // Just remove the lock so it can be retried
            await _cache.RemoveAsync(cacheKey, cancellationToken);
            _logger.LogDebug("Removed idempotency lock for failed key {Key} (retry allowed)", key);
            return;
        }

        var record = new IdempotencyRecord
        {
            Key = key,
            Status = IdempotencyStatus.Failed,
            StartedAt = DateTimeOffset.UtcNow,
            CompletedAt = DateTimeOffset.UtcNow,
            ErrorMessage = errorMessage
        };

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = _options.FailedResultTtl
        };

        await _cache.SetStringAsync(
            cacheKey,
            JsonSerializer.Serialize(record, JsonOptions),
            options,
            cancellationToken);

        _logger.LogDebug("Marked idempotency key {Key} as failed: {Error}", key, errorMessage);
    }

    public async Task<T?> GetResultAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        var cacheKey = GetCacheKey(key);
        var value = await _cache.GetStringAsync(cacheKey, cancellationToken);

        if (value == null)
            return null;

        var record = JsonSerializer.Deserialize<IdempotencyRecord>(value, JsonOptions);
        
        if (record?.ResultJson == null)
            return null;

        return JsonSerializer.Deserialize<T>(record.ResultJson, JsonOptions);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        var cacheKey = GetCacheKey(key);
        await _cache.RemoveAsync(cacheKey, cancellationToken);
        _logger.LogDebug("Removed idempotency entry for key {Key}", key);
    }

    private string GetCacheKey(string key) => $"{_options.KeyPrefix}{key}";

    private record IdempotencyRecord
    {
        public string Key { get; init; } = string.Empty;
        public IdempotencyStatus Status { get; init; }
        public DateTimeOffset StartedAt { get; init; }
        public DateTimeOffset? CompletedAt { get; init; }
        public string? ResultJson { get; init; }
        public string? ErrorMessage { get; init; }
    }
}

/// <summary>
/// Configuration options for idempotency service.
/// </summary>
public class IdempotencyOptions
{
    public const string SectionName = "Idempotency";

    /// <summary>
    /// Prefix for all idempotency cache keys.
    /// </summary>
    public string KeyPrefix { get; set; } = "idempotency:";

    /// <summary>
    /// Default TTL for processing locks.
    /// </summary>
    public TimeSpan DefaultLockTimeout { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Default TTL for successful results.
    /// </summary>
    public TimeSpan DefaultResultTtl { get; set; } = TimeSpan.FromHours(24);

    /// <summary>
    /// TTL for failed operation records.
    /// </summary>
    public TimeSpan FailedResultTtl { get; set; } = TimeSpan.FromHours(1);

    /// <summary>
    /// Whether to allow retrying failed operations with the same idempotency key.
    /// </summary>
    public bool AllowRetryOnFailure { get; set; } = true;
}
