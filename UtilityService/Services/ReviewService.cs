using Microsoft.EntityFrameworkCore;
using UtilityService.Data;
using UtilityService.Domain.Entities;
using UtilityService.DTOs;
using UtilityService.Interfaces;

namespace UtilityService.Services;

public class ReviewService : IReviewService
{
    private readonly UtilityDbContext _context;

    public ReviewService(UtilityDbContext context)
    {
        _context = context;
    }

    public async Task<ReviewDto> CreateReviewAsync(CreateReviewDto dto, string reviewerUsername, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders.FindAsync([dto.OrderId], cancellationToken)
            ?? throw new InvalidOperationException("Order not found");

        if (order.Status != OrderStatus.Delivered && order.Status != OrderStatus.Completed)
        {
            throw new InvalidOperationException("Cannot review an order that has not been delivered");
        }

        var existingReview = await _context.Reviews
            .FirstOrDefaultAsync(r => r.OrderId == dto.OrderId && r.ReviewerUsername == reviewerUsername, cancellationToken);

        if (existingReview != null)
        {
            throw new InvalidOperationException("You have already reviewed this order");
        }

        var isBuyer = order.BuyerUsername == reviewerUsername;
        var reviewedUsername = isBuyer ? order.SellerUsername : order.BuyerUsername;

        var review = new Review
        {
            Id = Guid.NewGuid(),
            OrderId = dto.OrderId,
            AuctionId = order.AuctionId,
            ReviewerUsername = reviewerUsername,
            ReviewedUsername = reviewedUsername,
            Type = isBuyer ? ReviewType.BuyerToSeller : ReviewType.SellerToBuyer,
            Rating = Math.Clamp(dto.Rating, 1, 5),
            Title = dto.Title,
            Comment = dto.Comment,
            IsVerifiedPurchase = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(review);
    }

    public async Task<ReviewDto?> GetReviewByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var review = await _context.Reviews.FindAsync([id], cancellationToken);
        return review != null ? MapToDto(review) : null;
    }

    public async Task<List<ReviewDto>> GetReviewsForUserAsync(string username, CancellationToken cancellationToken = default)
    {
        var reviews = await _context.Reviews
            .Where(r => r.ReviewedUsername == username)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        return reviews.Select(MapToDto).ToList();
    }

    public async Task<List<ReviewDto>> GetReviewsByUserAsync(string username, CancellationToken cancellationToken = default)
    {
        var reviews = await _context.Reviews
            .Where(r => r.ReviewerUsername == username)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        return reviews.Select(MapToDto).ToList();
    }

    public async Task<UserRatingSummaryDto> GetUserRatingSummaryAsync(string username, CancellationToken cancellationToken = default)
    {
        var reviews = await _context.Reviews
            .Where(r => r.ReviewedUsername == username)
            .ToListAsync(cancellationToken);

        if (reviews.Count == 0)
        {
            return new UserRatingSummaryDto(
                Username: username,
                AverageRating: 0,
                TotalReviews: 0,
                FiveStarCount: 0,
                FourStarCount: 0,
                ThreeStarCount: 0,
                TwoStarCount: 0,
                OneStarCount: 0,
                PositivePercentage: 0
            );
        }

        var averageRating = reviews.Average(r => r.Rating);
        var positiveReviews = reviews.Count(r => r.Rating >= 4);

        return new UserRatingSummaryDto(
            Username: username,
            AverageRating: Math.Round(averageRating, 1),
            TotalReviews: reviews.Count,
            FiveStarCount: reviews.Count(r => r.Rating == 5),
            FourStarCount: reviews.Count(r => r.Rating == 4),
            ThreeStarCount: reviews.Count(r => r.Rating == 3),
            TwoStarCount: reviews.Count(r => r.Rating == 2),
            OneStarCount: reviews.Count(r => r.Rating == 1),
            PositivePercentage: Math.Round((double)positiveReviews / reviews.Count * 100, 1)
        );
    }

    public async Task<ReviewDto> AddSellerResponseAsync(Guid reviewId, SellerResponseDto dto, string sellerUsername, CancellationToken cancellationToken = default)
    {
        var review = await _context.Reviews.FindAsync([reviewId], cancellationToken)
            ?? throw new InvalidOperationException("Review not found");

        if (review.ReviewedUsername != sellerUsername)
        {
            throw new UnauthorizedAccessException("You can only respond to reviews about yourself");
        }

        if (!string.IsNullOrEmpty(review.SellerResponse))
        {
            throw new InvalidOperationException("You have already responded to this review");
        }

        review.SellerResponse = dto.Response;
        review.SellerResponseAt = DateTimeOffset.UtcNow;
        review.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(review);
    }

    private static ReviewDto MapToDto(Review review)
    {
        return new ReviewDto(
            review.Id,
            review.OrderId,
            review.AuctionId,
            review.ReviewerUsername,
            review.ReviewedUsername,
            review.Type.ToString(),
            review.Rating,
            review.Title,
            review.Comment,
            review.IsVerifiedPurchase,
            review.SellerResponse,
            review.SellerResponseAt,
            review.CreatedAt
        );
    }
}
