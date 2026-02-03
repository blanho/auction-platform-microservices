using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Constants;

namespace Bidding.Application.Features.Bids.GetBidHistory;

public record GetBidHistoryQuery(
    Guid? AuctionId = null,
    Guid? UserId = null,
    BidStatus? Status = null,
    decimal? MinAmount = null,
    decimal? MaxAmount = null,
    DateTimeOffset? FromDate = null,
    DateTimeOffset? ToDate = null,
    int Page = PaginationDefaults.DefaultPage,
    int PageSize = PaginationDefaults.DefaultPageSize,
    string? SortBy = null,
    bool SortDescending = true
) : IQuery<PaginatedResult<BidHistoryItemDto>>;

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
