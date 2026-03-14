using Auctions.Application.DTOs;
namespace Auctions.Application.Features.Auctions.BuyNow;

public record BuyNowCommand(
    Guid AuctionId,
    Guid BuyerId,
    string BuyerUsername
) : ICommand<BuyNowResultDto>;

