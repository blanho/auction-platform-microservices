using Common.Contracts.Events;

namespace AuctionService.Contracts.Events;

public record AuctionDeletedEvent : IVersionedEvent
{
    public int Version => 1;
    public Guid Id { get; init; }
    public string Seller { get; init; } = string.Empty;
}
