namespace Common.Messaging.Events;

public class BidRejectedEvent
{
    public Guid BidId { get; set; }
    public Guid AuctionId { get; set; }
    public Guid BidderId { get; set; }
    public string BidderUsername { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTimeOffset RejectedAt { get; set; }
}
