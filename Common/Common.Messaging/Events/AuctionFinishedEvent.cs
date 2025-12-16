namespace Common.Messaging.Events;

public class AuctionFinishedEvent
{
    public bool ItemSold { get; set; }
    public Guid AuctionId { get; set; }
    public Guid? WinnerId { get; set; }
    public string? WinnerUsername { get; set; }
    public Guid SellerId { get; set; }
    public string SellerUsername { get; set; } = string.Empty;
    public decimal? SoldAmount { get; set; }
}
