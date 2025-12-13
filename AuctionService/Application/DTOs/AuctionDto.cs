namespace AuctionService.Application.DTOs
{
    public class AuctionDto
    {
        public Guid Id { get; set; }
        public int ReservePrice { get; set; }
        public int? BuyNowPrice { get; set; }
        public bool IsBuyNowAvailable { get; set; }
        public required string Seller { get; set; }
        public string? Winner { get; set; }
        public int? SoldAmount { get; set; }
        public int? CurrentHighBid { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public DateTimeOffset AuctionEnd { get; set; }
        public required string Status { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required string Make { get; set; }
        public required string Model { get; set; }
        public int Year { get; set; }
        public required string Color { get; set; }
        public int Mileage { get; set; }
        public Guid? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? CategorySlug { get; set; }
        public string? CategoryIcon { get; set; }
        public bool IsFeatured { get; set; }
        public List<AuctionFileDto> Files { get; set; } = new();
    }

    public class AuctionFileDto
    {
        public Guid StorageFileId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long Size { get; set; }
        public string? Url { get; set; }
        public string FileType { get; set; } = "image";
        public int DisplayOrder { get; set; }
        public bool IsPrimary { get; set; }
    }
}
