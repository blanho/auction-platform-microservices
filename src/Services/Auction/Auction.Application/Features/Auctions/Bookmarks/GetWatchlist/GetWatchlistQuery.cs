using Auctions.Application.DTOs;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Constants;

namespace Auctions.Application.Queries.Bookmarks.GetWatchlist;

public record GetWatchlistQuery(
    string Username,
    Guid? AuctionId = null,
    bool? NotifyOnBid = null,
    bool? NotifyOnEnd = null,
    DateTimeOffset? FromDate = null,
    DateTimeOffset? ToDate = null,
    int Page = PaginationDefaults.DefaultPage,
    int PageSize = PaginationDefaults.DefaultPageSize,
    string? SortBy = null,
    bool SortDescending = true
) : IQuery<PaginatedResult<BookmarkItemDto>>;

