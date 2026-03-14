using Common.Contracts.Events;

namespace SearchService.Contracts;

public record SearchIndexRebuiltEvent : IVersionedEvent
{
    public int Version => 1;

    public string IndexName { get; init; } = string.Empty;
    public int DocumentCount { get; init; }
    public DateTimeOffset RebuiltAt { get; init; }
    public TimeSpan Duration { get; init; }
}

public record SearchIndexErrorEvent : IVersionedEvent
{
    public int Version => 1;

    public string IndexName { get; init; } = string.Empty;
    public Guid DocumentId { get; init; }
    public string ErrorMessage { get; init; } = string.Empty;
    public DateTimeOffset OccurredAt { get; init; }
}
