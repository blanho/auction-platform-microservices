namespace Bidding.Domain.Events;

public record AutoBidActivatedDomainEvent : DomainEvent
{
    public Guid AutoBidId { get; init; }
    public Guid AuctionId { get; init; }
    public Guid UserId { get; init; }
}
