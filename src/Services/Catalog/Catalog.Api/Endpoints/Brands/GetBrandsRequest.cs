using BuildingBlocks.Application.Constants;

namespace Catalog.Api.Endpoints.Brands;

public sealed record GetBrandsRequest(
    bool ActiveOnly = true,
    bool FeaturedOnly = false,
    int Page = PaginationDefaults.DefaultPage,
    int PageSize = PaginationDefaults.DefaultPageSize,
    string? Search = null,
    string? SortBy = null,
    string? SortOrder = null);
