using Common.Contracts.Events;

namespace AuctionService.Contracts.Events;

public record AuctionCancelledEvent : IVersionedEvent
{
    public int Version => 1;
    public Guid AuctionId { get; init; }
    public Guid SellerId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
    public DateTimeOffset CancelledAt { get; init; }
}
