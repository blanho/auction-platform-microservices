using Common.Contracts.Events;

namespace AuctionService.Contracts.Events;

public record BulkAuctionUpdateCompletedEvent : IVersionedEvent
{
    public int Version => 1;
    public Guid CorrelationId { get; init; }
    public Guid RequestedBy { get; init; }
    public int TotalRequested { get; init; }
    public int SucceededCount { get; init; }
    public int FailedCount { get; init; }
    public bool Activated { get; init; }
    public string? Reason { get; init; }
    public TimeSpan Duration { get; init; }
    public DateTimeOffset CompletedAt { get; init; }
}
