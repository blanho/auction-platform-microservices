using Auctions.Application.DTOs;
using BuildingBlocks.Application.CQRS;

namespace Auctions.Application.Features.Reviews.CreateReview;

public record CreateReviewCommand(
    Guid AuctionId,
    Guid? OrderId,
    Guid ReviewerId,
    string ReviewerUsername,
    Guid ReviewedUserId,
    string ReviewedUsername,
    int Rating,
    string? Title,
    string? Comment
) : ICommand<ReviewDto>;

