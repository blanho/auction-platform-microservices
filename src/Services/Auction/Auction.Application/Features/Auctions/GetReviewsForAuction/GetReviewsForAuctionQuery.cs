using Auctions.Application.DTOs;
namespace Auctions.Application.Queries.GetReviewsForAuction;

public record GetReviewsForAuctionQuery(Guid AuctionId) : IQuery<List<ReviewDto>>;

