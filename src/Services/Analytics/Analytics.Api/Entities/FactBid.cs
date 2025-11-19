namespace Analytics.Api.Entities;

public class FactBid
{

    public Guid EventId { get; set; }

    public DateTimeOffset EventTime { get; set; }

    public DateTimeOffset IngestedAt { get; set; }

    public Guid AuctionId { get; set; }
    public Guid BidderId { get; set; }

    public DateOnly DateKey { get; set; }

    public string BidderUsername { get; set; } = string.Empty;

    public decimal BidAmount { get; set; }

    public string BidStatus { get; set; } = string.Empty;

    public short EventVersion { get; set; }
}
