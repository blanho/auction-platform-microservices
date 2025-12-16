using AuctionService.Application.DTOs;
using Common.CQRS.Abstractions;

namespace AuctionService.Application.Commands.UpdateBrand;

public record UpdateBrandCommand(
    Guid Id,
    string? Name,
    string? LogoUrl,
    string? Description,
    int? DisplayOrder,
    bool? IsActive,
    bool? IsFeatured
) : ICommand<BrandDto>;
