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
    public string OutbidBidder { get; set; } = string.Empty;
    public string NewHighBidder { get; set; } = string.Empty;
    public int NewHighBid { get; set; }
    public int PreviousBid { get; set; }
    public DateTime OutbidAt { get; set; }
}
