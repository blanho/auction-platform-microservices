using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Messaging.Events
{
    public class BidPlacedEvent
    {
        public Guid Id { get; set; }
        public Guid AuctionId { get; set; }
        public string Bidder { get; set; }
        public DateTime BidTime { get; set; }
        public int BidAmount { get; set; }
        public string BidStatus { get; set; }
    }
}
