namespace AuctionService.Application.DTOs
{
    public class FlashSaleDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? BannerUrl { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public int DiscountPercentage { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
        public List<FlashSaleItemDto> Items { get; set; } = new();
    }

    public class FlashSaleItemDto
    {
        public Guid Id { get; set; }
        public Guid FlashSaleId { get; set; }
        public Guid AuctionId { get; set; }
        public int? SpecialPrice { get; set; }
        public int? DiscountPercentage { get; set; }
        public int DisplayOrder { get; set; }
        public AuctionDto? Auction { get; set; }
    }

    public class CreateFlashSaleDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? BannerUrl { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public int DiscountPercentage { get; set; }
        public int DisplayOrder { get; set; } = 0;
    }

    public class UpdateFlashSaleDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? BannerUrl { get; set; }
        public DateTimeOffset? StartTime { get; set; }
        public DateTimeOffset? EndTime { get; set; }
        public int? DiscountPercentage { get; set; }
        public bool? IsActive { get; set; }
        public int? DisplayOrder { get; set; }
    }

    public class AddFlashSaleItemDto
    {
        public Guid AuctionId { get; set; }
        public int? SpecialPrice { get; set; }
        public int? DiscountPercentage { get; set; }
        public int DisplayOrder { get; set; } = 0;
    }

    public class ActiveFlashSaleDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? BannerUrl { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public int DiscountPercentage { get; set; }
        public long RemainingSeconds { get; set; }
        public List<FlashSaleAuctionDto> Auctions { get; set; } = new();
    }

    public class FlashSaleAuctionDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal SalePrice { get; set; }
        public int DiscountPercentage { get; set; }
        public int SoldCount { get; set; }
        public int TotalCount { get; set; }
    }
}
