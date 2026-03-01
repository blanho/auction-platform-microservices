using BuildingBlocks.Domain.Events;

namespace Auctions.Domain.Events;

public record AuctionHighBidUpdatedDomainEvent : DomainEvent
{
    public Guid AuctionId { get; init; }
    public decimal BidAmount { get; init; }
    public Guid? BidderId { get; init; }
    public string? BidderUsername { get; init; }
}
