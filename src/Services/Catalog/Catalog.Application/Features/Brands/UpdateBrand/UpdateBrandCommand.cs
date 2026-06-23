namespace Catalog.Application.Features.Brands.UpdateBrand;

public record UpdateBrandCommand(
    Guid Id,
    string? Name,
    string? Description,
    int? DisplayOrder,
    bool? IsActive,
    bool? IsFeatured) : ICommand<BrandDto>;
