#nullable enable
using Auctions.Domain.Entities;
using Auctions.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Abstractions.Auditing;

namespace Auctions.Infrastructure.Persistence.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly AuctionDbContext _context;
    private readonly IDateTimeProvider _dateTime;
    private readonly IAuditContext _auditContext;

    public ReviewRepository(AuctionDbContext context, IDateTimeProvider dateTime, IAuditContext auditContext)
    {
        _context = context;
        _dateTime = dateTime;
        _auditContext = auditContext;
    }

    public async Task<PaginatedResult<Review>> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Reviews
            .Where(x => !x.IsDeleted)
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<Review>(items, totalCount, page, pageSize);
    }

    public async Task<Review?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Reviews
            .Where(x => !x.IsDeleted)
            .Include(x => x.Auction)
                .ThenInclude(a => a!.Item)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<Review?> GetByAuctionAndReviewerAsync(Guid auctionId, string reviewerUsername, CancellationToken cancellationToken = default)
    {
        return await _context.Reviews
            .Where(x => !x.IsDeleted)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.AuctionId == auctionId && x.ReviewerUsername == reviewerUsername, cancellationToken);
    }

    public async Task<List<Review>> GetByAuctionIdAsync(Guid auctionId, CancellationToken cancellationToken = default)
    {
        return await _context.Reviews
            .Where(x => !x.IsDeleted && x.AuctionId == auctionId)
            .Include(x => x.Auction)
                .ThenInclude(a => a!.Item)
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Review>> GetByReviewedUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _context.Reviews
            .Where(x => !x.IsDeleted && x.ReviewedUsername == username)
            .Include(x => x.Auction)
                .ThenInclude(a => a!.Item)
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Review>> GetByReviewerUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _context.Reviews
            .Where(x => !x.IsDeleted && x.ReviewerUsername == username)
            .Include(x => x.Auction)
                .ThenInclude(a => a!.Item)
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<(double AverageRating, int TotalReviews)> GetRatingSummaryAsync(string username, CancellationToken cancellationToken = default)
    {
        var reviews = await _context.Reviews
            .Where(x => !x.IsDeleted && x.ReviewedUsername == username)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        if (reviews.Count == 0)
            return (0, 0);

        return (reviews.Average(x => x.Rating), reviews.Count);
    }

    public async Task<Review> CreateAsync(Review review, CancellationToken cancellationToken = default)
    {
        review.CreatedAt = _dateTime.UtcNow;
        review.CreatedBy = _auditContext.UserId;
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
        // Use tracked query - GetByIdAsync returns AsNoTracking which can't be updated
        var review = await _context.Reviews
            .Where(x => !x.IsDeleted)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        
        if (review is null)
        {
            return;
        }

        review.IsDeleted = true;
        review.UpdatedAt = _dateTime.UtcNow;
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Reviews
            .Where(x => !x.IsDeleted)
            .AnyAsync(x => x.Id == id, cancellationToken);
    }
}

