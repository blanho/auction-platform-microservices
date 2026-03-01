namespace Bidding.Domain.Events;

public record BidAcceptedBelowReserveDomainEvent : DomainEvent
{
    public Guid BidId { get; init; }
    public Guid AuctionId { get; init; }
    public Guid BidderId { get; init; }
    public string BidderUsername { get; init; } = string.Empty;
    public decimal Amount { get; init; }
}
