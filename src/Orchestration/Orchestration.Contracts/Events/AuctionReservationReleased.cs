using Common.Contracts.Events;

namespace OrchestrationService.Contracts.Events;

public record AuctionReservationReleased : IVersionedEvent
{
    public int Version => 1;
    public Guid CorrelationId { get; init; }
    public Guid AuctionId { get; init; }
    public DateTimeOffset ReleasedAt { get; init; }
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
}
