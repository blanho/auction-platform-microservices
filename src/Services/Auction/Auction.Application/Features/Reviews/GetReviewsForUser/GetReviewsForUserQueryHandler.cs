using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Application.Abstractions;
using Auctions.Application.DTOs;
using Auctions.Application.Interfaces;

namespace Auctions.Application.Features.Reviews.GetReviewsForUser;

public class GetReviewsForUserQueryHandler : IQueryHandler<GetReviewsForUserQuery, List<ReviewDto>>
{
    private readonly IReviewRepository _reviewRepository;

    public GetReviewsForUserQueryHandler(IReviewRepository reviewRepository)
    {
        _reviewRepository = reviewRepository;
    }

    public async Task<Result<List<ReviewDto>>> Handle(GetReviewsForUserQuery request, CancellationToken cancellationToken)
    {
        var reviews = await _reviewRepository.GetByReviewedUsernameAsync(request.Username, cancellationToken);

        var reviewDtos = reviews.Select(r => new ReviewDto(
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

        return Result.Success(reviewDtos);
    }
}
