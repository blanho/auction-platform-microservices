using Auctions.Application.DTOs;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Constants;

namespace Auctions.Application.Queries.GetReviewsForUser;

public record GetReviewsForUserQuery(
    string Username,
    int? MinRating = null,
    int? MaxRating = null,
    int Page = PaginationDefaults.DefaultPage,
    int PageSize = PaginationDefaults.DefaultPageSize,
    string? SortBy = null,
    bool SortDescending = true
) : IQuery<PaginatedResult<ReviewDto>>;

