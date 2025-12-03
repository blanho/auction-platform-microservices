using Common.Domain.Entities;
using Common.Domain.Enums;

namespace BidService.Domain.Entities
{
    public class Bid : BaseEntity
    {
        public Guid AuctionId { get; set; }
        public string Bidder { get; set; } = string.Empty;
        public int Amount { get; set; }
        public DateTime BidTime { get; set; }
        public BidStatus Status { get; set; }
    }

    public enum BidStatus
    {
        Pending,
        Accepted,
        AcceptedBelowReserve,
        TooLow,
        Rejected
    }
}
