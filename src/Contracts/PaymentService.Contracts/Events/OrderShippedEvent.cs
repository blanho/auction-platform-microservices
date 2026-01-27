namespace PaymentService.Contracts.Events;

public record OrderShippedEvent
{
    public Guid OrderId { get; init; }
    public Guid AuctionId { get; init; }
    public Guid BuyerId { get; init; }
    public string BuyerUsername { get; init; } = string.Empty;
    public string TrackingNumber { get; init; } = string.Empty;
    public string ShippingCarrier { get; init; } = string.Empty;
    public DateTimeOffset ShippedAt { get; init; }
}
