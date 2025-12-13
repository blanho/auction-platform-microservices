#nullable enable
using Common.Domain.Entities;
using Common.Domain.Enums;

namespace AuctionService.Domain.Entities
{
    public class Auction : BaseEntity
    {
        public int ReversePrice { get; set; } = 0;
        public int? BuyNowPrice { get; set; }
        public bool IsBuyNowEnabled => BuyNowPrice.HasValue && BuyNowPrice > 0;
        public bool IsBuyNowAvailable => IsBuyNowEnabled && Status == Status.Live && !SoldAmount.HasValue;
        public required string Seller { get; set; }
        public string? Winner { get; set; }
        public int? SoldAmount { get; set; }
        public int? CurrentHighBid { get; set; }
        public DateTimeOffset AuctionEnd { get; set; }
        public Status Status { get; set; }
        public bool IsFeatured { get; set; } = false;
        public required Item Item { get; set; }
    }
}
