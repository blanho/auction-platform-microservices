using Payment.Domain.Entities;

namespace Payment.Domain.Events;

public record OrderStatusChangedDomainEvent : DomainEvent
{
    public Guid OrderId { get; init; }
    public Guid AuctionId { get; init; }
    public Guid BuyerId { get; init; }
    public string BuyerUsername { get; init; } = string.Empty;
    public OrderStatus OldStatus { get; init; }
    public OrderStatus NewStatus { get; init; }
}
