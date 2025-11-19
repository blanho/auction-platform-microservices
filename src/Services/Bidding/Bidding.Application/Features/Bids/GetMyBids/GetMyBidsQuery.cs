using Bidding.Application.DTOs;
using BuildingBlocks.Application.CQRS;

namespace Bidding.Application.Features.Bids.Queries.GetMyBids;

public record GetMyBidsQuery(string Username) : IQuery<List<BidDto>>;
