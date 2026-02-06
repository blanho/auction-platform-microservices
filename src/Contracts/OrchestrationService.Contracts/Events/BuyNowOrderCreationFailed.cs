using Common.Contracts.Events;

namespace OrchestrationService.Contracts.Events;

public record BuyNowOrderCreationFailed : IVersionedEvent
{
    public int Version => 1;
    public Guid CorrelationId { get; init; }
    public Guid AuctionId { get; init; }
    public string Reason { get; init; } = string.Empty;
    public DateTimeOffset FailedAt { get; init; }
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
}
