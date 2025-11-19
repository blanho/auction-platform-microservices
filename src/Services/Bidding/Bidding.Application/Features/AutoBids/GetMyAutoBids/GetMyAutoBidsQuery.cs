namespace Bidding.Application.Features.AutoBids.GetMyAutoBids;

public record GetMyAutoBidsQuery(
    Guid UserId,
    bool? ActiveOnly = null,
    int Page = 1,
    int PageSize = 20) : IQuery<MyAutoBidsResult>;

public record MyAutoBidsResult
{
    public List<MyAutoBidDto> Items { get; init; } = new();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int ActiveCount { get; init; }
    public decimal TotalCommitted { get; init; }
}

public record MyAutoBidDto
{
    public Guid Id { get; init; }
    public Guid AuctionId { get; init; }
    public string AuctionTitle { get; init; } = string.Empty;
    public string AuctionStatus { get; init; } = string.Empty;
    public decimal MaxAmount { get; init; }
    public decimal CurrentBidAmount { get; init; }
    public decimal CurrentAuctionBid { get; init; }
    public bool IsActive { get; init; }
    public bool IsWinning { get; init; }
    public DateTimeOffset? AuctionEndTime { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
