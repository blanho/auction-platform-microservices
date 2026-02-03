using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.CQRS;
using Auctions.Application.DTOs;
using Auctions.Application.Filtering;
using Auctions.Application.Interfaces;

namespace Auctions.Application.Features.Reviews.GetReviewsByUser;

public class GetReviewsByUserQueryHandler : IQueryHandler<GetReviewsByUserQuery, PaginatedResult<ReviewDto>>
{
    private readonly IReviewRepository _reviewRepository;

    public GetReviewsByUserQueryHandler(IReviewRepository reviewRepository)
    {
        _reviewRepository = reviewRepository;
    }

    public async Task<Result<PaginatedResult<ReviewDto>>> Handle(GetReviewsByUserQuery request, CancellationToken cancellationToken)
    {
        var queryParams = new ReviewQueryParams
        {
            Page = request.Page,
            PageSize = request.PageSize,
            SortBy = request.SortBy,
            SortDescending = request.SortDescending,
            Filter = new ReviewFilter
            {
                ReviewerUsername = request.Username,
                AuctionId = request.AuctionId,
                MinRating = request.MinRating,
                MaxRating = request.MaxRating,
                HasSellerResponse = request.HasSellerResponse,
                FromDate = request.FromDate,
                ToDate = request.ToDate
            }
        };

        var result = await _reviewRepository.GetByReviewerUsernameAsync(queryParams, cancellationToken);

        var reviewDtos = result.Items.Select(r => new ReviewDto(
            r.Id,
            r.AuctionId,
            r.OrderId,
            r.ReviewerUsername,
            r.ReviewedUsername,
            r.Rating,
            r.Title,
            r.Comment,
            r.SellerResponse,
            r.SellerResponseAt,
            r.CreatedAt
        )).ToList();

        return Result.Success(new PaginatedResult<ReviewDto>(
            reviewDtos, result.TotalCount, result.Page, result.PageSize));
    }
}
