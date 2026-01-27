using Auctions.Application.DTOs;
using BuildingBlocks.Application.CQRS;

namespace Auctions.Application.Features.Brands.GetBrands;

public record GetBrandsQuery(
    bool ActiveOnly = true,
    bool FeaturedOnly = false,
    int? Count = null
) : IQuery<List<BrandDto>>;

