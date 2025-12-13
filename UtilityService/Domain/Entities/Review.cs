namespace UtilityService.Domain.Entities;

public class Review
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid AuctionId { get; set; }
    public string ReviewerUsername { get; set; } = string.Empty;
    public string ReviewedUsername { get; set; } = string.Empty;
    public ReviewType Type { get; set; }
    public int Rating { get; set; }
    public string? Title { get; set; }
    public string? Comment { get; set; }
    public bool IsVerifiedPurchase { get; set; } = true;
    public string? SellerResponse { get; set; }
    public DateTimeOffset? SellerResponseAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public enum ReviewType
{
    BuyerToSeller,
    SellerToBuyer
}
