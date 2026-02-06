using Auctions.Application.DTOs;
namespace Auctions.Application.Commands.ActivateAuction;

public record ActivateAuctionCommand(Guid AuctionId, Guid UserId) : ICommand<AuctionDto>;

