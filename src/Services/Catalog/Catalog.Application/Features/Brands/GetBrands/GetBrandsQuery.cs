namespace Catalog.Application.Features.Brands.GetBrands;

public record GetBrandsQuery(
    bool ActiveOnly = true,
    bool FeaturedOnly = false,
    int? Count = null) : IQuery<List<BrandDto>>;
