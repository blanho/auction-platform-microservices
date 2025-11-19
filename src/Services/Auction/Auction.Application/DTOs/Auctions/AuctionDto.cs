namespace Auctions.Application.DTOs
{
    public class AuctionDto
    {
        public Guid Id { get; set; }
        public decimal ReservePrice { get; set; }
        public decimal? BuyNowPrice { get; set; }
        public bool IsBuyNowAvailable { get; set; }
        public string Currency { get; set; } = "USD";
        public Guid SellerId { get; set; }
        public required string Seller { get; set; }
        public Guid? WinnerId { get; set; }
        public string? Winner { get; set; }
        public decimal? SoldAmount { get; set; }
        public decimal? CurrentHighBid { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public DateTimeOffset AuctionEnd { get; set; }
        public required string Status { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public string? Condition { get; set; }
        public int? YearManufactured { get; set; }
        public Dictionary<string, string>? Attributes { get; set; }
        public Guid? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? CategorySlug { get; set; }
        public string? CategoryIcon { get; set; }
        public bool IsFeatured { get; set; }
        public List<AuctionFileDto> Files { get; set; } = new();
    }

    public class AuctionFileDto
    {
        public Guid FileId { get; set; }
        public string FileType { get; set; } = "image";
        public int DisplayOrder { get; set; }
        public bool IsPrimary { get; set; }
    }

    public class BulkUpdateAuctionsDto
    {
        public List<Guid> AuctionIds { get; set; } = new();
        public bool Activate { get; set; }
        public string? Reason { get; set; }
    }
}

