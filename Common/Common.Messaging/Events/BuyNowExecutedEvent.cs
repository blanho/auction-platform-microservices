namespace Common.Messaging.Events
{
    public class BuyNowExecutedEvent
    {
        public Guid AuctionId { get; set; }
        public string Buyer { get; set; } = string.Empty;
        public string Seller { get; set; } = string.Empty;
        public int BuyNowPrice { get; set; }
        public string ItemTitle { get; set; } = string.Empty;
        public DateTime ExecutedAt { get; set; }
    }
}
