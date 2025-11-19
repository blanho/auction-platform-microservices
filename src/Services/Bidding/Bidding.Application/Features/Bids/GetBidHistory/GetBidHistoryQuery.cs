namespace Bidding.Application.Features.Bids.GetBidHistory;

public record GetBidHistoryQuery(
    Guid? AuctionId = null,
    Guid? UserId = null,
    BidStatus? Status = null,
    DateTimeOffset? FromDate = null,
    DateTimeOffset? ToDate = null,
    int Page = 1,
    int PageSize = 20) : IQuery<BidHistoryResult>;

public record BidHistoryResult
{
    public List<BidHistoryItemDto> Items { get; init; } = new();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public BidHistorySummary Summary { get; init; } = new();
}

public record BidHistoryItemDto
{
    public Guid Id { get; init; }
    public Guid AuctionId { get; init; }
    public Guid BidderId { get; init; }
    public string BidderUsername { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public DateTimeOffset BidTime { get; init; }
    public string Status { get; init; } = string.Empty;
}

public record BidHistorySummary
{
    public int TotalBids { get; init; }
    public int AcceptedBids { get; init; }
    public int RejectedBids { get; init; }
    public decimal TotalAmountBid { get; init; }
    public decimal HighestBid { get; init; }
    public decimal AverageBid { get; init; }
}
