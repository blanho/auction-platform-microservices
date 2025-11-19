using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Infrastructure.Messaging;

public interface IEventStormProtection
{

    Task<bool> ShouldProcessEventAsync(
        string eventType,
        string resourceId,
        TimeSpan debounceWindow,
        CancellationToken cancellationToken = default);

    Task<EventDebounceResult> DebounceAsync<T>(
        string eventType,
        string resourceId,
        T eventData,
        TimeSpan debounceWindow,
        CancellationToken cancellationToken = default);
}

public class EventStormProtection : IEventStormProtection
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<EventStormProtection> _logger;
    private const string KeyPrefix = "event:storm:";
    private const string DebouncePrefix = "event:debounce:";

    public EventStormProtection(
        IDistributedCache cache,
        ILogger<EventStormProtection> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<bool> ShouldProcessEventAsync(
        string eventType,
        string resourceId,
        TimeSpan debounceWindow,
        CancellationToken cancellationToken = default)
    {
        var key = $"{KeyPrefix}{eventType}:{resourceId}";

        var existing = await _cache.GetStringAsync(key, cancellationToken);
        if (!string.IsNullOrEmpty(existing))
        {
            _logger.LogDebug(
                "Event {EventType} for {ResourceId} debounced within {Window}ms window",
                eventType, resourceId, debounceWindow.TotalMilliseconds);
            return false;
        }

        await _cache.SetStringAsync(
            key,
            DateTimeOffset.UtcNow.ToString("o"),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = debounceWindow
            },
            cancellationToken);

        return true;
    }

    public async Task<EventDebounceResult> DebounceAsync<T>(
        string eventType,
        string resourceId,
        T eventData,
        TimeSpan debounceWindow,
        CancellationToken cancellationToken = default)
    {
        var key = $"{DebouncePrefix}{eventType}:{resourceId}";
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var data = System.Text.Json.JsonSerializer.Serialize(new DebounceEntry<T>
        {
            Timestamp = timestamp,
            Data = eventData
        });

        await _cache.SetStringAsync(
            key,
            data,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = debounceWindow.Add(TimeSpan.FromSeconds(5))
            },
            cancellationToken);

        var countKey = $"{key}:count";
        var countStr = await _cache.GetStringAsync(countKey, cancellationToken);
        var count = string.IsNullOrEmpty(countStr) ? 0 : int.Parse(countStr);
        count++;

        await _cache.SetStringAsync(
            countKey,
            count.ToString(),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = debounceWindow.Add(TimeSpan.FromSeconds(5))
            },
            cancellationToken);

        _logger.LogDebug(
            "Event {EventType} for {ResourceId} queued for debouncing (count: {Count})",
            eventType, resourceId, count);

        return new EventDebounceResult(
            ShouldProcessImmediately: count == 1,
            EventCount: count,
            Key: key);
    }

    private class DebounceEntry<T>
    {
        public long Timestamp { get; set; }
        public T? Data { get; set; }
    }
}

public record EventDebounceResult(
    bool ShouldProcessImmediately,
    int EventCount,
    string Key);

public class EventStormConfig
{
    public string EventType { get; set; } = string.Empty;
    public TimeSpan DebounceWindow { get; set; } = TimeSpan.FromMilliseconds(100);
    public int MaxEventsPerWindow { get; set; } = 10;
    public bool EnableBatching { get; set; } = false;
}
