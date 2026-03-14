using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Constants;

namespace Bidding.Application.Features.Bids.GetWinningBids;

public record GetWinningBidsQuery(
    Guid UserId,
    Guid? AuctionId = null,
    bool? IsPaid = null,
    DateTimeOffset? FromDate = null,
    DateTimeOffset? ToDate = null,
    int Page = PaginationDefaults.DefaultPage,
    int PageSize = PaginationDefaults.DefaultPageSize,
    string? SortBy = null,
    bool SortDescending = true
) : IQuery<PaginatedResult<WinningBidDto>>;

public record WinningBidDto
{
    public Guid BidId { get; init; }
    public Guid AuctionId { get; init; }
    public string AuctionTitle { get; init; } = string.Empty;
    public decimal WinningAmount { get; init; }
    public DateTimeOffset WonAt { get; init; }
    public string PaymentStatus { get; init; } = string.Empty;
    public bool IsPaid { get; init; }
}
