using AuctionService.Application.DTOs;
using Common.CQRS.Abstractions;

namespace AuctionService.Application.Commands.BuyNow;

public record BuyNowCommand(
    Guid AuctionId,
    Guid BuyerId,
    string BuyerUsername
) : ICommand<BuyNowResultDto>;
