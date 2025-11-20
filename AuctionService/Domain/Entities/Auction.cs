using Common.Domain.Entities;

namespace AuctionService.Domain.Entities
{
    public class Auction : BaseEntity
    {
        public int ReversePrice { get; set; } = 0;
        public string Seller { get; set; }
        public string Winner { get; set; }
        public int? SoldAmount { get; set; }
        public int? CurrentHighBid { get; set; }
        public DateTimeOffset AuctionEnd { get; set; }
        public Status Status { get; set; }
        public Item Item { get; set; }
    }
}
