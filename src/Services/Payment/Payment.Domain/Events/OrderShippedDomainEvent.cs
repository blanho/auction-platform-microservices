
namespace Payment.Domain.Events;

public record OrderShippedDomainEvent : DomainEvent
{
    public Guid OrderId { get; init; }
    public Guid AuctionId { get; init; }
    public Guid BuyerId { get; init; }
    public string BuyerUsername { get; init; } = string.Empty;
    public string? TrackingNumber { get; init; }
    public string? ShippingCarrier { get; init; }
}
