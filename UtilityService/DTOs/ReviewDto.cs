namespace UtilityService.DTOs;

public record ReviewDto(
    Guid Id,
    Guid OrderId,
    Guid AuctionId,
    string ReviewerUsername,
    string ReviewedUsername,
    string Type,
    int Rating,
    string? Title,
    string? Comment,
    bool IsVerifiedPurchase,
    string? SellerResponse,
    DateTimeOffset? SellerResponseAt,
    DateTimeOffset CreatedAt
);

public record CreateReviewDto(
    Guid OrderId,
    int Rating,
    string? Title,
    string? Comment
);

public record SellerResponseDto(
    string Response
);

public record UserRatingSummaryDto(
    string Username,
    double AverageRating,
    int TotalReviews,
    int FiveStarCount,
    int FourStarCount,
    int ThreeStarCount,
    int TwoStarCount,
    int OneStarCount,
    double PositivePercentage
);
