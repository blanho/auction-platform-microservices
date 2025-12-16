using AuctionService.Application.DTOs;
using Common.CQRS.Abstractions;

namespace AuctionService.Application.Commands.AddFlashSaleItem;

public record AddFlashSaleItemCommand(
    Guid FlashSaleId,
    Guid AuctionId,
    int? SpecialPrice,
    int? DiscountPercentage,
    int DisplayOrder
) : ICommand<FlashSaleItemDto>;
