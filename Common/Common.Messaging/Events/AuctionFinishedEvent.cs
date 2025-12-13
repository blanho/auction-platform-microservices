using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Messaging.Events
{
    public class AuctionFinishedEvent
    {
        public bool ItemSold { get; set; }
        public Guid AuctionId { get; set; }
        public string Winner { get; set; }
        public  string Seller { get; set; }
        public int? SoldAmount { get; set; }
    }
}
