using Auctions.Application.Filtering;
using Auctions.Domain.Entities;
using BuildingBlocks.Application.Abstractions;

namespace Auctions.Application.Interfaces;

public interface IReviewRepository : IRepository<Review>
{
    Task<Review?> GetByAuctionAndReviewerAsync(Guid auctionId, string reviewerUsername, CancellationToken cancellationToken = default);
    Task<PaginatedResult<Review>> GetByAuctionIdAsync(ReviewQueryParams queryParams, CancellationToken cancellationToken = default);
    Task<PaginatedResult<Review>> GetByReviewedUsernameAsync(ReviewQueryParams queryParams, CancellationToken cancellationToken = default);
    Task<PaginatedResult<Review>> GetByReviewerUsernameAsync(ReviewQueryParams queryParams, CancellationToken cancellationToken = default);
    Task<(double AverageRating, int TotalReviews)> GetRatingSummaryAsync(string username, CancellationToken cancellationToken = default);
}

