namespace Common.Messaging.Events;

public class AuctionCreatedEvent
{
    public Guid Id { get; set; }
    public Guid SellerId { get; set; }
    public string SellerUsername { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? AuctionEnd { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal ReservePrice { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal? SoldAmount { get; set; }
    public decimal? CurrentHighBid { get; set; }
    public Guid? WinnerId { get; set; }
    public string? WinnerUsername { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Condition { get; set; }
    public int? YearManufactured { get; set; }
}
