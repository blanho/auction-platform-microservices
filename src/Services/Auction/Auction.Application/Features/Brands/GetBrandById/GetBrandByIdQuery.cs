using Auctions.Application.DTOs;
using BuildingBlocks.Application.CQRS;

namespace Auctions.Application.Features.Brands.GetBrandById;

public record GetBrandByIdQuery(Guid Id) : IQuery<BrandDto>;

