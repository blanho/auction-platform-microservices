namespace AuctionService.Application.DTOs;

public record ReviewDto(
    Guid Id,
    Guid AuctionId,
    Guid? OrderId,
    string ReviewerUsername,
    string ReviewedUsername,
    int Rating,
    string? Title,
    string? Comment,
    string? SellerResponse,
    DateTimeOffset? SellerResponseAt,
    DateTimeOffset CreatedAt
);

public record CreateReviewDto(
    Guid AuctionId,
    Guid? OrderId,
    string ReviewedUsername,
    int Rating,
    string? Title,
    string? Comment
);

public record UpdateReviewDto(
    int? Rating,
    string? Title,
    string? Comment
);

public record AddSellerResponseDto(
    string Response
);

public record UserRatingSummaryDto(
    string Username,
    double AverageRating,
    int TotalReviews
);
