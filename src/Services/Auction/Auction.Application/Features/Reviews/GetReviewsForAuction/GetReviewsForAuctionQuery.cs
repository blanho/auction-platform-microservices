using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Constants;
using BuildingBlocks.Application.CQRS;
using Auctions.Application.DTOs;

namespace Auctions.Application.Features.Reviews.GetReviewsForAuction;

public record GetReviewsForAuctionQuery(
    Guid AuctionId,
    int? MinRating = null,
    int? MaxRating = null,
    bool? HasSellerResponse = null,
    DateTimeOffset? FromDate = null,
    DateTimeOffset? ToDate = null,
    int Page = PaginationDefaults.DefaultPage,
    int PageSize = PaginationDefaults.DefaultPageSize,
    string? SortBy = null,
    bool SortDescending = true
) : IQuery<PaginatedResult<ReviewDto>>;
