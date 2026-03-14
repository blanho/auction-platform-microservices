using Common.Contracts.Events;

namespace BidService.Contracts.Events;

public record BidEscrowRequestedEvent : IVersionedEvent
{
    public int Version => 1;
    public Guid BidId { get; init; }
    public Guid AuctionId { get; init; }
    public string Bidder { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public DateTimeOffset RequestedAt { get; init; }
}
