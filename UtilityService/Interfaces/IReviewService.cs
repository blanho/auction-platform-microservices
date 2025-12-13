using UtilityService.DTOs;

namespace UtilityService.Interfaces;

public interface IReviewService
{
    Task<ReviewDto> CreateReviewAsync(CreateReviewDto dto, string reviewerUsername, CancellationToken cancellationToken = default);
    Task<ReviewDto?> GetReviewByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<ReviewDto>> GetReviewsForUserAsync(string username, CancellationToken cancellationToken = default);
    Task<List<ReviewDto>> GetReviewsByUserAsync(string username, CancellationToken cancellationToken = default);
    Task<UserRatingSummaryDto> GetUserRatingSummaryAsync(string username, CancellationToken cancellationToken = default);
    Task<ReviewDto> AddSellerResponseAsync(Guid reviewId, SellerResponseDto dto, string sellerUsername, CancellationToken cancellationToken = default);
}
