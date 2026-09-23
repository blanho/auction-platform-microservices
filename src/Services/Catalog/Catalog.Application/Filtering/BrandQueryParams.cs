using BuildingBlocks.Application.Paging;

namespace Catalog.Application.Filtering;

public class BrandQueryParams : QueryParameters
{
    public bool ActiveOnly { get; init; } = true;
    public bool FeaturedOnly { get; init; }
    public string? Search { get; init; }
}
