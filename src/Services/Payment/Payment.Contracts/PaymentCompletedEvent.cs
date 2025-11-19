namespace Payment.Contracts;

public record PaymentCompletedEvent
{
    public Guid OrderId { get; init; }
    public Guid AuctionId { get; init; }
    public Guid BuyerId { get; init; }
    public string BuyerUsername { get; init; } = string.Empty;
    public Guid SellerId { get; init; }
    public string SellerUsername { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string TransactionId { get; init; } = string.Empty;
    public DateTimeOffset PaidAt { get; init; }
}
