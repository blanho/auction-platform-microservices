using AuctionService.Application.DTOs;
using Common.CQRS.Abstractions;

namespace AuctionService.Application.Queries.GetReviewsByUser;

public record GetReviewsByUserQuery(string Username) : IQuery<List<ReviewDto>>;
