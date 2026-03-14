using Common.Contracts.Events;

namespace OrchestrationService.Contracts.Events;

public record BuyNowSagaCompleted : IVersionedEvent
{
    public int Version => 1;
    public Guid CorrelationId { get; init; }
    public Guid AuctionId { get; init; }
    public Guid OrderId { get; init; }
    public bool Success { get; init; }
    public string? FailureReason { get; init; }
    public DateTimeOffset CompletedAt { get; init; }
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
}
