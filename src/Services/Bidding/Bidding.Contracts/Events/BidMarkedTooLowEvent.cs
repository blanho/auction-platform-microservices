using Common.Contracts.Events;

namespace BidService.Contracts.Events;

public record BidMarkedTooLowEvent : IVersionedEvent
{
    public int Version => 1;

    public Guid BidId { get; init; }
    public Guid AuctionId { get; init; }
    public Guid BidderId { get; init; }
    public decimal Amount { get; init; }
    public DateTimeOffset MarkedAt { get; init; }
}
