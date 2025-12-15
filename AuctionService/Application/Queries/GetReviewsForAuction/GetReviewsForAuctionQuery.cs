using AuctionService.Application.DTOs;
using Common.CQRS.Abstractions;

namespace AuctionService.Application.Queries.GetReviewsForAuction;

public record GetReviewsForAuctionQuery(Guid AuctionId) : IQuery<List<ReviewDto>>;
