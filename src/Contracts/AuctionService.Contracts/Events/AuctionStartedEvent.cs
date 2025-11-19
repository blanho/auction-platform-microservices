namespace AuctionService.Contracts.Events;

public record AuctionStartedEvent : IVersionedEvent
{
    public int Version => 1;
    public Guid AuctionId { get; init; }
    public string Seller { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public DateTime StartTime { get; init; }
    public DateTime EndTime { get; init; }
    public decimal ReservePrice { get; init; }
}
