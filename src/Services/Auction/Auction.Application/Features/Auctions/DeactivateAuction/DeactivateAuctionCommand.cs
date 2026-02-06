using Auctions.Application.DTOs;

namespace Auctions.Application.Commands.DeactivateAuction;

public record DeactivateAuctionCommand(
    Guid AuctionId,    Guid UserId,    string? Reason = null
) : ICommand<AuctionDto>;

