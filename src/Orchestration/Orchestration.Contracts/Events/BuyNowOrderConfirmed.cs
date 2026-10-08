using Common.Contracts.Events;

namespace OrchestrationService.Contracts.Events;

public record BuyNowOrderConfirmed : IVersionedEvent
{
    public int Version => 1;
    public Guid CorrelationId { get; init; }
    public Guid AuctionId { get; init; }
    public Guid OrderId { get; init; }
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
}
