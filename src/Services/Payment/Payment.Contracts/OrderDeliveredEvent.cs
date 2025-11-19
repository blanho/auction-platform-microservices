namespace Payment.Contracts;

public record OrderDeliveredEvent
{
    public Guid OrderId { get; init; }
    public Guid AuctionId { get; init; }
    public Guid BuyerId { get; init; }
    public string BuyerUsername { get; init; } = string.Empty;
    public Guid SellerId { get; init; }
    public string SellerUsername { get; init; } = string.Empty;
    public DateTimeOffset DeliveredAt { get; init; }
}
