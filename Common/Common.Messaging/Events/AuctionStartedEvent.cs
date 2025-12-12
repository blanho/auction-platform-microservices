namespace Common.Messaging.Events;

/// <summary>
/// Event published when a scheduled auction starts (goes live)
/// </summary>
public class AuctionStartedEvent
{
    public Guid AuctionId { get; set; }
    public string Seller { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int ReservePrice { get; set; }
}
