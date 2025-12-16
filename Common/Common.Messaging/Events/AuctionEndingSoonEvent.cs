namespace Common.Messaging.Events;

public class AuctionEndingSoonEvent
{
    public Guid AuctionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal CurrentHighBid { get; set; }
    public DateTime EndTime { get; set; }
    public string TimeRemaining { get; set; } = string.Empty;
    public List<string> WatcherUsernames { get; set; } = new();
}
