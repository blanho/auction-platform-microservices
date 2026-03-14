using Bidding.Application.Features.AutoBids.GetAutoBid;
using BuildingBlocks.Application.CQRS;

namespace Bidding.Application.Features.AutoBids.GetAutoBidForAuction;

public record GetAutoBidForAuctionQuery(
    Guid AuctionId,
    Guid UserId) : IQuery<AutoBidDetailDto?>;
