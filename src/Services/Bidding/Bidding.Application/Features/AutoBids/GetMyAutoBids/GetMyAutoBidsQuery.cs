using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Constants;

namespace Bidding.Application.Features.AutoBids.GetMyAutoBids;

public record GetMyAutoBidsQuery(
    Guid UserId,
    Guid? AuctionId = null,
    bool? IsActive = null,
    decimal? MinMaxAmount = null,
    decimal? MaxMaxAmount = null,
    DateTimeOffset? FromDate = null,
    DateTimeOffset? ToDate = null,
    int Page = PaginationDefaults.DefaultPage,
    int PageSize = PaginationDefaults.DefaultPageSize,
    string? SortBy = null,
    bool SortDescending = true
) : IQuery<PaginatedResult<MyAutoBidDto>>;

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
