#nullable enable
using Common.Domain.Entities;

namespace AuctionService.Domain.Entities
{
    public class FlashSale : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? BannerUrl { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public int DiscountPercentage { get; set; }
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; } = 0;
        public ICollection<FlashSaleItem> Items { get; set; } = new List<FlashSaleItem>();
    }

    public class FlashSaleItem
    {
        public Guid Id { get; set; }
        public Guid FlashSaleId { get; set; }
        public FlashSale? FlashSale { get; set; }
        public Guid AuctionId { get; set; }
        public Auction? Auction { get; set; }
        public decimal? SpecialPrice { get; set; }
        public int? DiscountPercentage { get; set; }
        public int DisplayOrder { get; set; } = 0;
        public DateTimeOffset AddedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
