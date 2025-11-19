namespace AuctionService.Contracts.Events;

public record AuctionEndingSoonEvent : IVersionedEvent
{
    public int Version => 1;
    public Guid AuctionId { get; init; }
    public string Title { get; init; } = string.Empty;
    public decimal CurrentHighBid { get; init; }
    public DateTime EndTime { get; init; }
    public string TimeRemaining { get; init; } = string.Empty;
    public List<string> WatcherUsernames { get; init; } = new();
}
