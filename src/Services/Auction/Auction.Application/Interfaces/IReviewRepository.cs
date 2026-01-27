using Auctions.Domain.Entities;
using Microsoft.Extensions.Logging;
using BuildingBlocks.Infrastructure.Caching;
using BuildingBlocks.Infrastructure.Repository;

namespace Auctions.Application.Interfaces;

public interface IReviewRepository : IRepository<Review>
{
    Task<Review?> GetByAuctionAndReviewerAsync(Guid auctionId, string reviewerUsername, CancellationToken cancellationToken = default);
    Task<List<Review>> GetByAuctionIdAsync(Guid auctionId, CancellationToken cancellationToken = default);
    Task<List<Review>> GetByReviewedUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<List<Review>> GetByReviewerUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<(double AverageRating, int TotalReviews)> GetRatingSummaryAsync(string username, CancellationToken cancellationToken = default);
}

