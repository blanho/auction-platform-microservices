using Auctions.Application.DTOs;
namespace Auctions.Application.Features.Brands.CreateBrand;

public record CreateBrandCommand(
    string Name,
    string? LogoUrl,
    string? Description,
    int DisplayOrder,
    bool IsFeatured
) : ICommand<BrandDto>;

