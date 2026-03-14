using Common.Contracts.Events;

namespace BidService.Contracts.Events;

public record AutoBidCreatedEvent : IVersionedEvent
{
    public int Version => 1;

    public Guid AutoBidId { get; init; }
    public Guid AuctionId { get; init; }
    public Guid UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public decimal MaxAmount { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
