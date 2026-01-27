using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Application.Abstractions;
using Auctions.Application.DTOs;
using Auctions.Application.Interfaces;

namespace Auctions.Application.Features.Reviews.GetReviewsByUser;

public class GetReviewsByUserQueryHandler : IQueryHandler<GetReviewsByUserQuery, List<ReviewDto>>
{
    private readonly IReviewRepository _reviewRepository;

    public GetReviewsByUserQueryHandler(IReviewRepository reviewRepository)
    {
        _reviewRepository = reviewRepository;
    }

    public async Task<Result<List<ReviewDto>>> Handle(GetReviewsByUserQuery request, CancellationToken cancellationToken)
    {
        var reviews = await _reviewRepository.GetByReviewerUsernameAsync(request.Username, cancellationToken);

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
