using Auctions.Application.DTOs;
namespace Auctions.Application.Commands.DeactivateAuction;

public record DeactivateAuctionCommand(
    Guid AuctionId,
    string? Reason = null
) : ICommand<AuctionDto>;

