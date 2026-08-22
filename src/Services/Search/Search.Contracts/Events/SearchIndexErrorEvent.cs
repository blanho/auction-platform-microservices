using Common.Contracts.Events;

namespace SearchService.Contracts;

public record SearchIndexErrorEvent : IVersionedEvent
{
    public int Version => 1;

    public string IndexName { get; init; } = string.Empty;
    public Guid DocumentId { get; init; }
    public string ErrorMessage { get; init; } = string.Empty;
    public DateTimeOffset OccurredAt { get; init; }
}
