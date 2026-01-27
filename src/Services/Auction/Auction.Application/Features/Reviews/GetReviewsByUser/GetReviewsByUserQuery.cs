using BuildingBlocks.Application.CQRS;
using Auctions.Application.DTOs;

namespace Auctions.Application.Features.Reviews.GetReviewsByUser;

public record GetReviewsByUserQuery(string Username) : IQuery<List<ReviewDto>>;
