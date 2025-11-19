namespace BidService.Contracts.Events;

public record BidEscrowReleasedEvent : IVersionedEvent
{
    public int Version => 1;
    public Guid AuctionId { get; init; }
    public string Bidder { get; init; } = string.Empty;
    public decimal ReleasedAmount { get; init; }
    public string Reason { get; init; } = string.Empty;
    public DateTime ReleasedAt { get; init; }
}
