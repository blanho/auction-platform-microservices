using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Application.Abstractions;
using Auctions.Application.DTOs;
using Auctions.Application.Interfaces;

namespace Auctions.Application.Features.Reviews.GetReviewsForAuction;

public class GetReviewsForAuctionQueryHandler : IQueryHandler<GetReviewsForAuctionQuery, List<ReviewDto>>
{
    private readonly IReviewRepository _reviewRepository;

    public GetReviewsForAuctionQueryHandler(IReviewRepository reviewRepository)
    {
        _reviewRepository = reviewRepository;
    }

    public async Task<Result<List<ReviewDto>>> Handle(GetReviewsForAuctionQuery request, CancellationToken cancellationToken)
    {
        var reviews = await _reviewRepository.GetByAuctionIdAsync(request.AuctionId, cancellationToken);

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
