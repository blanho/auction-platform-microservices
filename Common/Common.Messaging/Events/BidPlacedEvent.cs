namespace Common.Messaging.Events;

public class BidPlacedEvent
{
    public Guid Id { get; set; }
    public Guid AuctionId { get; set; }
    public Guid BidderId { get; set; }
    public string Bidder { get; set; } = string.Empty;
    public DateTimeOffset BidTime { get; set; }
    public decimal BidAmount { get; set; }
    public string BidStatus { get; set; } = string.Empty;
}