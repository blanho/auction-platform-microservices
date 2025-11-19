using Auctions.Application.DTOs;
namespace Auctions.Application.Queries.GetBrands;

public record GetBrandsQuery(
    bool ActiveOnly = true,
    bool FeaturedOnly = false,
    int? Count = null
) : IQuery<List<BrandDto>>;

