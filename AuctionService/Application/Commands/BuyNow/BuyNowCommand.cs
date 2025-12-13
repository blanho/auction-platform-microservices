using AuctionService.Application.DTOs;
using Common.CQRS.Abstractions;

namespace AuctionService.Application.Commands.BuyNow;

public record BuyNowCommand(
    Guid AuctionId,
    string Buyer
) : ICommand<BuyNowResultDto>;
