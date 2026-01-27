using BuildingBlocks.Application.CQRS;
using Auctions.Application.DTOs;

namespace Auctions.Application.Features.Reviews.GetReviewsForAuction;

public record GetReviewsForAuctionQuery(Guid AuctionId) : IQuery<List<ReviewDto>>;
