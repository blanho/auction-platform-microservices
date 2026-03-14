using Common.Contracts.Events;

namespace BidService.Contracts.Events;

public record AutoBidUpdatedEvent : IVersionedEvent
{
    public int Version => 1;

    public Guid AutoBidId { get; init; }
    public Guid AuctionId { get; init; }
    public Guid UserId { get; init; }
    public decimal NewMaxAmount { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}
