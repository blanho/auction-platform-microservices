using Bidding.Application.DTOs;
using BuildingBlocks.Application.CQRS;

namespace Bidding.Application.Features.Bids.Commands.PlaceBid;

public record PlaceBidCommand(
    Guid AuctionId,
    decimal Amount,
    Guid BidderId,
    string BidderUsername,
    string IdempotencyKey
) : ICommand<BidDto>;
