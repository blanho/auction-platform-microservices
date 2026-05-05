using Bidding.Domain.Constants;
using Bidding.Domain.Enums;

namespace Bidding.Application.DTOs;

public record BidHistoryFilterRequest(
    Guid? AuctionId,
    BidStatus? Status,
    DateTimeOffset? FromDate,
    DateTimeOffset? ToDate,
    int Page = BidDefaults.DefaultPage,
    int PageSize = BidDefaults.DefaultPageSize);

public record WinningBidsRequest
{
    public int Page { get; init; } = BidDefaults.DefaultPage;
    public int PageSize { get; init; } = BidDefaults.DefaultPageSize;
}

public record BidStatsRequest
{
    public int Days { get; init; } = BidDefaults.DefaultDaysForStats;
    public int TopLimit { get; init; } = BidDefaults.DefaultTopBiddersLimit;
}

public record AutoBidsFilterRequest
{
    public bool? ActiveOnly { get; init; }
    public int Page { get; init; } = BidDefaults.DefaultPage;
    public int PageSize { get; init; } = BidDefaults.DefaultPageSize;
}
