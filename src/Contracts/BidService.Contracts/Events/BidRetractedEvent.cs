namespace BidService.Contracts.Events;

public record BidRetractedEvent : IVersionedEvent
{
    public int Version => 1;

    public Guid BidId { get; init; }
    public Guid AuctionId { get; init; }
    public Guid BidderId { get; init; }
    public string Bidder { get; init; } = string.Empty;
    public decimal BidAmount { get; init; }
    public decimal RetractedAmount { get; init; }
    public string Reason { get; init; } = string.Empty;
    public DateTimeOffset RetractedAt { get; init; }

    public Guid? NewHighestBidId { get; init; }
    public decimal? NewHighestAmount { get; init; }
    public Guid? NewHighestBidderId { get; init; }
}
