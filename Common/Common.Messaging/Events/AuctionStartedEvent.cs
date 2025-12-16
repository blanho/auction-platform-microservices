namespace Common.Messaging.Events;

public class AuctionStartedEvent
{
    public Guid AuctionId { get; set; }
    public string Seller { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public decimal ReservePrice { get; set; }
}
