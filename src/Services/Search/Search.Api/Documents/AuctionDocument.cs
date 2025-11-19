namespace Search.Api.Documents;

public class AuctionDocument
{

    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public List<string> Tags { get; set; } = new();

    public Guid? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string? CategorySlug { get; set; }

    public List<string> CategoryPath { get; set; } = new();

    public Guid? BrandId { get; set; }
    public string? BrandName { get; set; }

    public Guid SellerId { get; set; }
    public string SellerUsername { get; set; } = string.Empty;
    public string? SellerDisplayName { get; set; }
    public double? SellerRating { get; set; }

    public decimal StartPrice { get; set; }

    public decimal ReservePrice { get; set; }

    public decimal CurrentPrice { get; set; }

    public decimal? BuyNowPrice { get; set; }

    public string Currency { get; set; } = "USD";

    public string Status { get; set; } = string.Empty;

    public string? Condition { get; set; }

    public int? YearManufactured { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? StartTime { get; set; }
    public DateTimeOffset? EndTime { get; set; }
    public DateTimeOffset? SoldAt { get; set; }

    public DateTimeOffset LastSyncedAt { get; set; }

    public int BidCount { get; set; }
    public int ViewCount { get; set; }
    public int WatchCount { get; set; }

    public string? ThumbnailUrl { get; set; }
    public List<string> ImageUrls { get; set; } = new();

    public Guid? WinnerId { get; set; }
    public string? WinnerUsername { get; set; }
    public decimal? FinalPrice { get; set; }

    public Guid? WinningBidderId { get => WinnerId; set => WinnerId = value; }

    public string? WinningBidderUsername { get => WinnerUsername; set => WinnerUsername = value; }

    public decimal? WinningBidAmount { get => FinalPrice; set => FinalPrice = value; }

    public bool IsFeatured { get; set; }

    public GeoLocation? Location { get; set; }

    public Dictionary<string, object>? Attributes { get; set; }
}

 public class GeoLocation
{
    public double Lat { get; set; }
    public double Lon { get; set; }
}
