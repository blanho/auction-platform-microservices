using Common.Contracts.Events;

namespace BidService.Contracts.Events;

public record HighestBidUpdatedEvent : IVersionedEvent
{
    public int Version => 1;
    public Guid Id { get; init; }
    public Guid AuctionId { get; init; }
    public Guid BidderId { get; init; }
    public string BidderUsername { get; init; } = string.Empty;
    public decimal NewHighestAmount { get; init; }
    public decimal? PreviousHighestAmount { get; init; }
    public DateTimeOffset BidTime { get; init; }
    public string BidStatus { get; init; } = string.Empty;
}
