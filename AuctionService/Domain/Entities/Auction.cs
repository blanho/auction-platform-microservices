#nullable enable
using Common.Domain.Entities;
using Common.Domain.Enums;

namespace AuctionService.Domain.Entities
{
    public class Auction : BaseEntity
    {
        public decimal ReservePrice { get; set; } = 0;
        public decimal? BuyNowPrice { get; set; }
        public string Currency { get; set; } = "USD";
        public bool IsBuyNowEnabled => BuyNowPrice.HasValue && BuyNowPrice > 0;
        public bool IsBuyNowAvailable => IsBuyNowEnabled && Status == Status.Live && !SoldAmount.HasValue;
        
        public Guid SellerId { get; set; }
        public string SellerUsername { get; set; } = string.Empty;
        public Guid? WinnerId { get; set; }
        public string? WinnerUsername { get; set; }
        
        public decimal? SoldAmount { get; set; }
        public decimal? CurrentHighBid { get; set; }
        public DateTimeOffset AuctionEnd { get; set; }
        public Status Status { get; set; }
        public bool IsFeatured { get; set; } = false;
        public required Item Item { get; set; }
        
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<UserAuctionBookmark> Bookmarks { get; set; } = new List<UserAuctionBookmark>();
        public ICollection<FlashSaleItem> FlashSaleItems { get; set; } = new List<FlashSaleItem>();
        public ICollection<AuctionQuestion> Questions { get; set; } = new List<AuctionQuestion>();
    }
}
