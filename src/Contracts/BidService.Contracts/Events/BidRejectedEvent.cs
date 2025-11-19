namespace BidService.Contracts.Events;

public record BidRejectedEvent : IVersionedEvent
{
    public int Version => 1;
    public Guid BidId { get; init; }
    public Guid AuctionId { get; init; }
    public Guid BidderId { get; init; }
    public string BidderUsername { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Reason { get; init; } = string.Empty;
    public DateTimeOffset RejectedAt { get; init; }
}
