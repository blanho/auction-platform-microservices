using BuildingBlocks.Domain.Events;

namespace Auctions.Domain.Events;

public record AuctionDeletedDomainEvent : DomainEvent
{
    public Guid AuctionId { get; init; }
    public Guid SellerId { get; init; }
}

