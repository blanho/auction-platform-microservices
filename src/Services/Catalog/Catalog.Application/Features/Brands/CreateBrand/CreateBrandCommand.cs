namespace Catalog.Application.Features.Brands.CreateBrand;

public record CreateBrandCommand(
    string Name,
    string? Description,
    int DisplayOrder,
    bool IsFeatured) : ICommand<BrandDto>;
