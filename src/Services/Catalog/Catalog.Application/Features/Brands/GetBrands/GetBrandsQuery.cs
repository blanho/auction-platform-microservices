namespace Catalog.Application.Features.Brands.GetBrands;

public record GetBrandsQuery(
    bool ActiveOnly = true,
    bool FeaturedOnly = false,
    int Page = PaginationDefaults.DefaultPage,
    int PageSize = PaginationDefaults.DefaultPageSize,
    string? Search = null,
    string? SortBy = null,
    string? SortOrder = null) : IQuery<PaginatedResult<BrandDto>>;
