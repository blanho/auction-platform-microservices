using BuildingBlocks.Application.CQRS;
using Auctions.Application.DTOs;

namespace Auctions.Application.Features.Reviews.GetReviewsForUser;

public record GetReviewsForUserQuery(string Username) : IQuery<List<ReviewDto>>;
