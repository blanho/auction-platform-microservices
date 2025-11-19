namespace BidService.Contracts.Events;

public record BidPlacedEvent : IVersionedEvent
{
    public int Version => 1;

    public Guid Id { get; init; }
    public Guid AuctionId { get; init; }
    public Guid BidderId { get; init; }
    public string Bidder { get; init; } = string.Empty;
    public DateTimeOffset BidTime { get; init; }
    public decimal BidAmount { get; init; }
    public string BidStatus { get; init; } = string.Empty;
}
