using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Application.Abstractions;
using Auctions.Application.DTOs;
using Auctions.Application.Interfaces;

namespace Auctions.Application.Features.Reviews.GetUserRatingSummary;

public class GetUserRatingSummaryQueryHandler : IQueryHandler<GetUserRatingSummaryQuery, UserRatingSummaryDto>
{
    private readonly IReviewRepository _reviewRepository;

    public GetUserRatingSummaryQueryHandler(IReviewRepository reviewRepository)
    {
        _reviewRepository = reviewRepository;
    }

    public async Task<Result<UserRatingSummaryDto>> Handle(GetUserRatingSummaryQuery request, CancellationToken cancellationToken)
    {
        var (averageRating, totalReviews) = await _reviewRepository.GetRatingSummaryAsync(request.Username, cancellationToken);

        var summary = new UserRatingSummaryDto(
            request.Username,
            averageRating,
            totalReviews
        );

        return Result.Success(summary);
    }
}
