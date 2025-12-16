using AuctionService.Application.DTOs;
using Common.CQRS.Abstractions;

namespace AuctionService.Application.Commands.UpdateFlashSale;

public record UpdateFlashSaleCommand(
    Guid Id,
    string? Title,
    string? Description,
    string? BannerUrl,
    DateTimeOffset? StartTime,
    DateTimeOffset? EndTime,
    int? DiscountPercentage,
    bool? IsActive,
    int? DisplayOrder
) : ICommand<FlashSaleDto>;
