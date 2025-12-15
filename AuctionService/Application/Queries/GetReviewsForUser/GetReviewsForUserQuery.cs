using AuctionService.Application.DTOs;
using Common.CQRS.Abstractions;

namespace AuctionService.Application.Queries.GetReviewsForUser;

public record GetReviewsForUserQuery(string Username) : IQuery<List<ReviewDto>>;
