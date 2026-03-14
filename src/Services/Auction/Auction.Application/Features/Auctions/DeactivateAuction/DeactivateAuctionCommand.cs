using Auctions.Application.DTOs;

namespace Auctions.Application.Features.Auctions.DeactivateAuction;

public record DeactivateAuctionCommand(
    Guid AuctionId,    Guid UserId,    string? Reason = null
) : ICommand<AuctionDto>;

