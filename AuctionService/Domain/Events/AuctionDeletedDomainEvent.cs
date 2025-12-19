using Common.Domain.Events;

namespace AuctionService.Domain.Events;

public record AuctionDeletedDomainEvent : DomainEvent
{
    public Guid AuctionId { get; init; }
    public Guid SellerId { get; init; }
}
