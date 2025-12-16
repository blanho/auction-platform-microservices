using AuctionService.Application.DTOs;
using Common.CQRS.Abstractions;

namespace AuctionService.Application.Commands.CreateReview;

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
