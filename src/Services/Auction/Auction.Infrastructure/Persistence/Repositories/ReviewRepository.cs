#nullable enable
using Auctions.Domain.Entities;
using Auctions.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using BuildingBlocks.Application.Constants;

namespace Auctions.Infrastructure.Persistence.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly AuctionDbContext _context;
    private readonly IDateTimeProvider _dateTime;

    public ReviewRepository(AuctionDbContext context, IDateTimeProvider dateTime)
    {
        _context = context;
        _dateTime = dateTime;
    }

    public async Task<List<Review>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Reviews
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Review> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var review = await _context.Reviews
            .Where(x => !x.IsDeleted)
            .Include(x => x.Auction)
                .ThenInclude(a => a!.Item)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        
        return review ?? throw new KeyNotFoundException($"Review with ID {id} not found");
    }

    public async Task<Review?> GetByAuctionAndReviewerAsync(Guid auctionId, string reviewerUsername, CancellationToken cancellationToken = default)
    {
        return await _context.Reviews
            .Where(x => !x.IsDeleted)
            .FirstOrDefaultAsync(x => x.AuctionId == auctionId && x.ReviewerUsername == reviewerUsername, cancellationToken);
    }

    public async Task<List<Review>> GetByAuctionIdAsync(Guid auctionId, CancellationToken cancellationToken = default)
    {
        return await _context.Reviews
            .Where(x => !x.IsDeleted && x.AuctionId == auctionId)
            .Include(x => x.Auction)
                .ThenInclude(a => a!.Item)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Review>> GetByReviewedUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _context.Reviews
            .Where(x => !x.IsDeleted && x.ReviewedUsername == username)
            .Include(x => x.Auction)
                .ThenInclude(a => a!.Item)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Review>> GetByReviewerUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _context.Reviews
            .Where(x => !x.IsDeleted && x.ReviewerUsername == username)
            .Include(x => x.Auction)
                .ThenInclude(a => a!.Item)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<(double AverageRating, int TotalReviews)> GetRatingSummaryAsync(string username, CancellationToken cancellationToken = default)
    {
        var reviews = await _context.Reviews
            .Where(x => !x.IsDeleted && x.ReviewedUsername == username)
            .ToListAsync(cancellationToken);

        if (reviews.Count == 0)
            return (0, 0);

        return (reviews.Average(x => x.Rating), reviews.Count);
    }

    public async Task<Review> CreateAsync(Review review, CancellationToken cancellationToken = default)
    {
        review.CreatedAt = _dateTime.UtcNow;
        review.CreatedBy = SystemGuids.System;
        review.IsDeleted = false;
        
        await _context.Reviews.AddAsync(review, cancellationToken);
        
        return review;
    }

    public Task UpdateAsync(Review review, CancellationToken cancellationToken = default)
    {
        review.UpdatedAt = _dateTime.UtcNow;
        _context.Reviews.Update(review);
        
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var review = await GetByIdAsync(id, cancellationToken);
        review.IsDeleted = true;
        review.UpdatedAt = _dateTime.UtcNow;
        _context.Reviews.Update(review);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Reviews
            .Where(x => !x.IsDeleted)
            .AnyAsync(x => x.Id == id, cancellationToken);
    }
}

