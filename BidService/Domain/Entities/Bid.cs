using Common.Domain.Entities;
using Common.Domain.Enums;

namespace BidService.Domain.Entities
{
    public class Bid : BaseEntity
    {
        public Guid AuctionId { get; set; }
        public Guid BidderId { get; set; }
        public string BidderUsername { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTimeOffset BidTime { get; set; }
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
