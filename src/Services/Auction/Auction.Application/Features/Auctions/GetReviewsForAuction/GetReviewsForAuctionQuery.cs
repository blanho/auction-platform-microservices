using Auctions.Application.DTOs;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Constants;

namespace Auctions.Application.Queries.GetReviewsForAuction;

public record GetReviewsForAuctionQuery(
    Guid AuctionId,
    int? MinRating = null,
    int? MaxRating = null,
    int Page = PaginationDefaults.DefaultPage,
    int PageSize = PaginationDefaults.DefaultPageSize,
    string? SortBy = null,
    bool SortDescending = true
) : IQuery<PaginatedResult<ReviewDto>>;

