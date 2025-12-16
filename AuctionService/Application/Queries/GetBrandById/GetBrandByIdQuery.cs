using AuctionService.Application.DTOs;
using Common.CQRS.Abstractions;

namespace AuctionService.Application.Queries.GetBrandById;

public record GetBrandByIdQuery(Guid Id) : IQuery<BrandDto>;
