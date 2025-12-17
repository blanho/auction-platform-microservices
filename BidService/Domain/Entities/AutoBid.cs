using Common.Domain.Entities;

namespace BidService.Domain.Entities
{
    public class AutoBid : BaseEntity
    {
        public Guid AuctionId { get; set; }
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public decimal MaxAmount { get; set; }
        public decimal CurrentBidAmount { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTimeOffset? LastBidAt { get; set; }
    }
}
