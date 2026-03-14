using Common.Contracts.Events;

namespace AuctionService.Contracts.Events;

public record AuctionImportCompletedEvent : IVersionedEvent
{
    public int Version => 1;
    public Guid CorrelationId { get; init; }
    public Guid SellerId { get; init; }
    public int TotalRows { get; init; }
    public int SucceededCount { get; init; }
    public int FailedCount { get; init; }
    public int SkippedDuplicateCount { get; init; }
    public TimeSpan Duration { get; init; }
    public DateTimeOffset CompletedAt { get; init; }
    public List<ImportRowErrorPayload> Errors { get; init; } = [];
}

public record ImportRowErrorPayload
{
    public int RowNumber { get; init; }
    public string Field { get; init; } = string.Empty;
    public string ErrorMessage { get; init; } = string.Empty;
}
