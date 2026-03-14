namespace Bidding.Domain.Events;

public record BidRetractedDomainEvent : DomainEvent
{
    public Guid BidId { get; init; }
    public Guid AuctionId { get; init; }
    public Guid BidderId { get; init; }
    public string BidderUsername { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Reason { get; init; } = string.Empty;
    public bool WasHighestBid { get; init; }
    public Guid? NewHighestBidId { get; init; }
    public decimal? NewHighestAmount { get; init; }
    public Guid? NewHighestBidderId { get; init; }
    public string? NewHighestBidderUsername { get; init; }
}
