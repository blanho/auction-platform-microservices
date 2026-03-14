using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Application.Abstractions;
using Auctions.Application.DTOs;
using Auctions.Application.Interfaces;
using Auctions.Application.Errors;

namespace Auctions.Application.Features.Reviews.GetReviewById;

public class GetReviewByIdQueryHandler : IQueryHandler<GetReviewByIdQuery, ReviewDto>
{
    private readonly IReviewRepository _reviewRepository;

    public GetReviewByIdQueryHandler(IReviewRepository reviewRepository)
    {
        _reviewRepository = reviewRepository;
    }

    public async Task<Result<ReviewDto>> Handle(GetReviewByIdQuery request, CancellationToken cancellationToken)
    {
        var review = await _reviewRepository.GetByIdAsync(request.Id, cancellationToken);

        if (review == null)
        {
            return Result.Failure<ReviewDto>(AuctionErrors.Review.NotFound);
        }

        return Result.Success(new ReviewDto(
            review.Id,
            review.AuctionId,
            review.OrderId,
            review.ReviewerUsername,
            review.ReviewedUsername,
            review.Rating,
            review.Title,
            review.Comment,
            review.SellerResponse,
            review.SellerResponseAt,
            review.CreatedAt
        ));
    }
}
