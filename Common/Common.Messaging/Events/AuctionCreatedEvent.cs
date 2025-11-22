namespace Common.Messaging.Events;

public class AuctionCreatedEvent
{
    public Guid Id { get; set; }
    public string Seller { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? AuctionEnd { get; set; }
    public string Status { get; set; } = string.Empty;
    public int ReservePrice { get; set; }
    public int? SoldAmount { get; set; }
    public int? CurrentHighBid { get; set; }
    public string? Winner { get; set; }
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public string Color { get; set; } = string.Empty;
    public int Mileage { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
}
