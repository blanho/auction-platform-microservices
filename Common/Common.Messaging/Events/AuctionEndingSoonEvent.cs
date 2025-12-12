namespace Common.Messaging.Events;

/// <summary>
/// Event published when an auction is ending soon to notify watchers
/// </summary>
public class AuctionEndingSoonEvent
{
    public Guid AuctionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int CurrentHighBid { get; set; }
    public DateTime EndTime { get; set; }
    public string TimeRemaining { get; set; } = string.Empty;
    public List<string> WatcherUsernames { get; set; } = new();
}
