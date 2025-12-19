using Common.Domain.Events;

namespace BidService.Domain.Events;

public record HighestBidUpdatedDomainEvent : DomainEvent
{
    public Guid AuctionId { get; init; }
    public Guid BidId { get; init; }
    public Guid BidderId { get; init; }
    public string BidderUsername { get; init; } = string.Empty;
    public decimal NewHighestAmount { get; init; }
    public decimal? PreviousHighestAmount { get; init; }
}
