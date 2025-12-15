using AuctionService.Application.DTOs;
using Common.CQRS.Abstractions;

namespace AuctionService.Application.Queries.GetReviewById;

public record GetReviewByIdQuery(Guid Id) : IQuery<ReviewDto>;
