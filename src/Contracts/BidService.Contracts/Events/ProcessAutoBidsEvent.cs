namespace BidService.Contracts.Events;

public record ProcessAutoBidsEvent : IVersionedEvent
{
    public int Version => 1;
    public Guid AuctionId { get; init; }
    public decimal CurrentHighBid { get; init; }
    public Guid CurrentHighBidderId { get; init; }
    public string CurrentHighBidderUsername { get; init; } = string.Empty;
    public DateTimeOffset TriggeredAt { get; init; }
}
