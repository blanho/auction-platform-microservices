using AuctionService.Application.DTOs;
using Common.CQRS.Abstractions;

namespace AuctionService.Application.Queries.GetBrands;

public record GetBrandsQuery(
    bool ActiveOnly = true,
    bool FeaturedOnly = false,
    int? Count = null
) : IQuery<List<BrandDto>>;
