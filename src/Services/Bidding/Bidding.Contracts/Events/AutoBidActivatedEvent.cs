using Common.Contracts.Events;

namespace BidService.Contracts.Events;

public record AutoBidActivatedEvent : IVersionedEvent
{
    public int Version => 1;

    public Guid AutoBidId { get; init; }
    public Guid AuctionId { get; init; }
    public Guid UserId { get; init; }
    public DateTimeOffset ActivatedAt { get; init; }
}
