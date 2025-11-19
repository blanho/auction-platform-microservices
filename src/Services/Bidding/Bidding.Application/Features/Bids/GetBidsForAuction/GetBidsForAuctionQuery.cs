using Bidding.Application.DTOs;
using BuildingBlocks.Application.CQRS;

namespace Bidding.Application.Features.Bids.Queries.GetBidsForAuction;

public record GetBidsForAuctionQuery(Guid AuctionId) : IQuery<List<BidDto>>;
