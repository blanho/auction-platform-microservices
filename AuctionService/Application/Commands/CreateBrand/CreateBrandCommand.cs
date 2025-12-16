using AuctionService.Application.DTOs;
using Common.CQRS.Abstractions;

namespace AuctionService.Application.Commands.CreateBrand;

public record CreateBrandCommand(
    string Name,
    string? LogoUrl,
    string? Description,
    int DisplayOrder,
    bool IsFeatured
) : ICommand<BrandDto>;
