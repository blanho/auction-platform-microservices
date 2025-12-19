namespace Common.Messaging.Events;

public class BidEscrowRequestedEvent
{
    public Guid BidId { get; set; }
    public Guid AuctionId { get; set; }
    public string Bidder { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime RequestedAt { get; set; }
}

public class BidEscrowConfirmedEvent
{
    public Guid BidId { get; set; }
    public Guid AuctionId { get; set; }
    public string Bidder { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? FailureReason { get; set; }
    public decimal HeldAmount { get; set; }
    public DateTime ConfirmedAt { get; set; }
}

public class BidEscrowReleasedEvent
{
    public Guid AuctionId { get; set; }
    public string Bidder { get; set; } = string.Empty;
    public decimal ReleasedAmount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime ReleasedAt { get; set; }
}

public class OutbidEvent
{
    public Guid AuctionId { get; set; }
    public Guid OutbidBidderId { get; set; }
    public string OutbidBidderUsername { get; set; } = string.Empty;
    public Guid NewHighBidderId { get; set; }
    public string NewHighBidderUsername { get; set; } = string.Empty;
    public decimal NewHighBidAmount { get; set; }
    public decimal PreviousBidAmount { get; set; }
    public DateTimeOffset OutbidAt { get; set; }
}
