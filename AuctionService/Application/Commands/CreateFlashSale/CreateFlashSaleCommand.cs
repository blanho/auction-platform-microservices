using AuctionService.Application.DTOs;
using Common.CQRS.Abstractions;

namespace AuctionService.Application.Commands.CreateFlashSale;

public record CreateFlashSaleCommand(
    string Title,
    string? Description,
    string? BannerUrl,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    int DiscountPercentage,
    int DisplayOrder
) : ICommand<FlashSaleDto>;
