using Bidding.Application.DTOs;
using BuildingBlocks.Application.CQRS;

namespace Bidding.Application.Features.Bids.GetBidIncrement;

public record GetBidIncrementQuery(decimal CurrentBid) : IQuery<BidIncrementInfoDto>;
