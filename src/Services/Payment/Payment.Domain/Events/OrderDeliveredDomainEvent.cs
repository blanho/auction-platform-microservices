
namespace Payment.Domain.Events;

public record OrderDeliveredDomainEvent : DomainEvent
{
    public Guid OrderId { get; init; }
    public Guid AuctionId { get; init; }
    public Guid BuyerId { get; init; }
    public string BuyerUsername { get; init; } = string.Empty;
    public Guid SellerId { get; init; }
    public string SellerUsername { get; init; } = string.Empty;
}
