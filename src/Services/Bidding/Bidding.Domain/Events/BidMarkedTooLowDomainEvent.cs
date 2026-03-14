namespace Bidding.Domain.Events;

public record BidMarkedTooLowDomainEvent : DomainEvent
{
    public Guid BidId { get; init; }
    public Guid AuctionId { get; init; }
    public Guid BidderId { get; init; }
    public decimal Amount { get; init; }
}
