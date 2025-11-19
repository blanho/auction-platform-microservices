

namespace Bidding.Domain.Events;

public record HighestBidUpdatedDomainEvent : DomainEvent
{
    public Guid AuctionId { get; init; }
    public Guid BidId { get; init; }
    public Guid BidderId { get; init; }
    public string BidderUsername { get; init; } = string.Empty;
    public decimal NewHighestAmount { get; init; }
    public decimal? PreviousHighestAmount { get; init; }
    public Guid? PreviousBidderId { get; init; }
    public string? PreviousBidderUsername { get; init; }
    public bool IsAutoBid { get; init; }
}
