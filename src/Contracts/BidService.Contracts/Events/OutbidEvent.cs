using Common.Contracts.Events;

namespace BidService.Contracts.Events;

public record OutbidEvent : IVersionedEvent
{
    public int Version => 1;
    public Guid AuctionId { get; init; }
    public Guid OutbidBidderId { get; init; }
    public string OutbidBidderUsername { get; init; } = string.Empty;
    public Guid NewHighBidderId { get; init; }
    public string NewHighBidderUsername { get; init; } = string.Empty;
    public decimal NewHighBidAmount { get; init; }
    public decimal PreviousBidAmount { get; init; }
    public DateTimeOffset OutbidAt { get; init; }
}
