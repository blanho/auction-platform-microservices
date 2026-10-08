namespace Auctions.Domain.Entities;

public class BuyNowPurchase
{
    public uint RowVersion { get; set; }
    public Guid CorrelationId { get; set; }
    public Guid AuctionId { get; set; }
    public Guid BuyerId { get; set; }
    public string Buyer { get; set; } = string.Empty;
    public string Seller { get; set; } = string.Empty;
    public string ItemTitle { get; set; } = string.Empty;
    public decimal BuyNowPrice { get; set; }
    public Guid? OrderId { get; set; }
    public string Status { get; set; } = "Processing";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? CompletedAt { get; set; }
}
