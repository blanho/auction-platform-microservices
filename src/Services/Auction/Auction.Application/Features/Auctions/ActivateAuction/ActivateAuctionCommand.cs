using Auctions.Application.DTOs;
namespace Auctions.Application.Features.Auctions.ActivateAuction;

public record ActivateAuctionCommand(Guid AuctionId, Guid UserId) : ICommand<AuctionDto>;

