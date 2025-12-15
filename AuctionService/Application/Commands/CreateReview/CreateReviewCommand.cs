using AuctionService.Application.DTOs;
using Common.CQRS.Abstractions;

namespace AuctionService.Application.Commands.CreateReview;

public record CreateReviewCommand(
    Guid AuctionId,
    Guid? OrderId,
    string ReviewerUsername,
    string ReviewedUsername,
    int Rating,
    string? Title,
    string? Comment
) : ICommand<ReviewDto>;
