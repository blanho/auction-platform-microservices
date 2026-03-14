using Common.Contracts.Events;

namespace AuctionService.Contracts.Events;

public record AuctionHighBidUpdatedEvent : IVersionedEvent
{
    public int Version => 1;

    public Guid AuctionId { get; init; }
    public decimal BidAmount { get; init; }
    public Guid? BidderId { get; init; }
    public string? BidderUsername { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}
