#nullable enable
using System.Linq.Expressions;
using Auctions.Application.Filtering;
using Auctions.Domain.Entities;
using Auctions.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Application.Filtering;
using BuildingBlocks.Application.Paging;

namespace Auctions.Infrastructure.Persistence.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly AuctionDbContext _context;
    private readonly IDateTimeProvider _dateTime;
    private readonly IAuditContext _auditContext;

    private static readonly Dictionary<string, Expression<Func<Review, object>>> ReviewSortMap = 
        new(StringComparer.OrdinalIgnoreCase)
    {
        ["createdat"] = x => x.CreatedAt,
        ["rating"] = x => x.Rating,
        ["title"] = x => x.Title
    };

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

    public async Task<PaginatedResult<Review>> GetByAuctionIdAsync(ReviewQueryParams queryParams, CancellationToken cancellationToken = default)
    {
        var filter = queryParams.Filter;
        
        var filterBuilder = FilterBuilder<Review>.Create()
            .When(true, x => !x.IsDeleted)
            .WhenHasValue(filter.AuctionId, x => x.AuctionId == filter.AuctionId!.Value)
            .WhenHasValue(filter.MinRating, x => x.Rating >= filter.MinRating!.Value)
            .WhenHasValue(filter.MaxRating, x => x.Rating <= filter.MaxRating!.Value)
            .WhenHasValue(filter.HasSellerResponse, x => filter.HasSellerResponse!.Value ? x.SellerResponse != null : x.SellerResponse == null)
            .WhenHasValue(filter.FromDate, x => x.CreatedAt >= filter.FromDate!.Value)
            .WhenHasValue(filter.ToDate, x => x.CreatedAt <= filter.ToDate!.Value);

        var query = _context.Reviews
            .Include(x => x.Auction)
                .ThenInclude(a => a!.Item)
            .AsNoTracking()
            .ApplyFiltering(filterBuilder)
            .ApplySorting(queryParams, ReviewSortMap, x => x.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .ApplyPaging(queryParams)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<Review>(items, totalCount, queryParams.Page, queryParams.PageSize);
    }

    public async Task<PaginatedResult<Review>> GetByReviewedUsernameAsync(ReviewQueryParams queryParams, CancellationToken cancellationToken = default)
    {
        var filter = queryParams.Filter;
        
        var filterBuilder = FilterBuilder<Review>.Create()
            .When(true, x => !x.IsDeleted)
            .WhenNotEmpty(filter.ReviewedUsername, x => x.ReviewedUsername == filter.ReviewedUsername)
            .WhenHasValue(filter.AuctionId, x => x.AuctionId == filter.AuctionId!.Value)
            .WhenHasValue(filter.MinRating, x => x.Rating >= filter.MinRating!.Value)
            .WhenHasValue(filter.MaxRating, x => x.Rating <= filter.MaxRating!.Value)
            .WhenHasValue(filter.HasSellerResponse, x => filter.HasSellerResponse!.Value ? x.SellerResponse != null : x.SellerResponse == null)
            .WhenHasValue(filter.FromDate, x => x.CreatedAt >= filter.FromDate!.Value)
            .WhenHasValue(filter.ToDate, x => x.CreatedAt <= filter.ToDate!.Value);

        var query = _context.Reviews
            .Include(x => x.Auction)
                .ThenInclude(a => a!.Item)
            .AsNoTracking()
            .ApplyFiltering(filterBuilder)
            .ApplySorting(queryParams, ReviewSortMap, x => x.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .ApplyPaging(queryParams)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<Review>(items, totalCount, queryParams.Page, queryParams.PageSize);
    }

    public async Task<PaginatedResult<Review>> GetByReviewerUsernameAsync(ReviewQueryParams queryParams, CancellationToken cancellationToken = default)
    {
        var filter = queryParams.Filter;
        
        var filterBuilder = FilterBuilder<Review>.Create()
            .When(true, x => !x.IsDeleted)
            .WhenNotEmpty(filter.ReviewerUsername, x => x.ReviewerUsername == filter.ReviewerUsername)
            .WhenHasValue(filter.AuctionId, x => x.AuctionId == filter.AuctionId!.Value)
            .WhenHasValue(filter.MinRating, x => x.Rating >= filter.MinRating!.Value)
            .WhenHasValue(filter.MaxRating, x => x.Rating <= filter.MaxRating!.Value)
            .WhenHasValue(filter.HasSellerResponse, x => filter.HasSellerResponse!.Value ? x.SellerResponse != null : x.SellerResponse == null)
            .WhenHasValue(filter.FromDate, x => x.CreatedAt >= filter.FromDate!.Value)
            .WhenHasValue(filter.ToDate, x => x.CreatedAt <= filter.ToDate!.Value);

        var query = _context.Reviews
            .Include(x => x.Auction)
                .ThenInclude(a => a!.Item)
            .AsNoTracking()
            .ApplyFiltering(filterBuilder)
            .ApplySorting(queryParams, ReviewSortMap, x => x.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .ApplyPaging(queryParams)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<Review>(items, totalCount, queryParams.Page, queryParams.PageSize);
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

