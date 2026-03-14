using Common.Contracts.Events;

namespace PaymentService.Contracts.Events;

public record OrderCreatedEvent : IVersionedEvent
{
    public int Version => 1;

    public Guid OrderId { get; init; }
    public Guid AuctionId { get; init; }
    public Guid BuyerId { get; init; }
    public string BuyerUsername { get; init; } = string.Empty;
    public Guid SellerId { get; init; }
    public string SellerUsername { get; init; } = string.Empty;
    public string ItemTitle { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
