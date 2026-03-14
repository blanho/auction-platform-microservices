using BuildingBlocks.Application.CQRS;
using Auctions.Application.DTOs;

namespace Auctions.Application.Features.Reviews.GetReviewById;

public record GetReviewByIdQuery(Guid Id) : IQuery<ReviewDto>;
