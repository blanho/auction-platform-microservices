using Common.Contracts.Events;

namespace PaymentService.Contracts.Events;

public record OrderShippedEvent : IVersionedEvent
{
    public int Version => 1;

    public Guid OrderId { get; init; }
    public Guid AuctionId { get; init; }
    public Guid BuyerId { get; init; }
    public string BuyerUsername { get; init; } = string.Empty;
    public string TrackingNumber { get; init; } = string.Empty;
    public string ShippingCarrier { get; init; } = string.Empty;
    public DateTimeOffset ShippedAt { get; init; }
}
