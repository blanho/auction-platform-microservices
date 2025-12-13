using Common.Domain.Entities;

namespace BidService.Domain.Entities
{
    public class AutoBid : BaseEntity
    {
        public Guid AuctionId { get; set; }
        public string Bidder { get; set; } = string.Empty;
        public int MaxAmount { get; set; }
        public int CurrentBidAmount { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastBidAt { get; set; }
    }
}
