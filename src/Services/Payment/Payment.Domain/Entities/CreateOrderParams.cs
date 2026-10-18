namespace Payment.Domain.Entities;

public sealed record CreateOrderParams
{
    public required Guid AuctionId { get; init; }
    public required Guid BuyerId { get; init; }
    public required string BuyerUsername { get; init; }
    public required Guid SellerId { get; init; }
    public required string SellerUsername { get; init; }
    public required string ItemTitle { get; init; }
    public required decimal WinningBid { get; init; }
    public decimal? PlatformFeePercent { get; init; }
    public decimal? ShippingCost { get; init; }
    public string? ShippingAddress { get; init; }
    public string? BuyerNotes { get; init; }
    public Guid? BuyNowCorrelationId { get; init; }
}
