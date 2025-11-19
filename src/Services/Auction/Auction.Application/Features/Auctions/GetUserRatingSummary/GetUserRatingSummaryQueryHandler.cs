using Auctions.Application.DTOs;
namespace Auctions.Application.Queries.GetUserRatingSummary;

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
        
        return Result.Success(new UserRatingSummaryDto(
            request.Username,
            Math.Round(averageRating, 2),
            totalReviews
        ));
    }
}

