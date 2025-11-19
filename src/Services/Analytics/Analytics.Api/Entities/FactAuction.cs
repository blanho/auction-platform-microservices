namespace Analytics.Api.Entities;

public class FactAuction
{

    public Guid EventId { get; set; }
    public Guid AuctionId { get; set; }
    public DateTimeOffset EventTime { get; set; }
    public DateTimeOffset IngestedAt { get; set; }

    public Guid SellerId { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? WinnerId { get; set; }
    public DateOnly DateKey { get; set; }

    public string Title { get; set; } = string.Empty;
    public string SellerUsername { get; set; } = string.Empty;
    public string? WinnerUsername { get; set; }
    public string? CategoryName { get; set; }
    public string[]? CategoryPath { get; set; }

    public decimal StartingPrice { get; set; }
    public decimal? ReservePrice { get; set; }
    public decimal? FinalPrice { get; set; }
    public decimal? BuyNowPrice { get; set; }
    public int TotalBids { get; set; }
    public int UniqueBidders { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? EndedAt { get; set; }
    public decimal? DurationHours { get; set; }

    public string Status { get; set; } = string.Empty;

    public bool Sold { get; set; }
    public bool HadReserve { get; set; }
    public bool ReserveMet { get; set; }
    public bool HadBuyNow { get; set; }
    public bool UsedBuyNow { get; set; }
    public short TimesExtended { get; set; }

    public string? Condition { get; set; }
    public string Currency { get; set; } = "USD";

    public string EventType { get; set; } = string.Empty;

    public short EventVersion { get; set; }
}
