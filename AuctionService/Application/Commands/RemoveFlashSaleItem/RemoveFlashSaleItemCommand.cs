using Common.CQRS.Abstractions;

namespace AuctionService.Application.Commands.RemoveFlashSaleItem;

public record RemoveFlashSaleItemCommand(
    Guid FlashSaleId,
    Guid AuctionId
) : ICommand;
