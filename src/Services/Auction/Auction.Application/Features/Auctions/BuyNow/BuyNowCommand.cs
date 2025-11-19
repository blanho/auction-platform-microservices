using Auctions.Application.DTOs;
namespace Auctions.Application.Commands.BuyNow;

public record BuyNowCommand(
    Guid AuctionId,
    Guid BuyerId,
    string BuyerUsername
) : ICommand<BuyNowResultDto>;

