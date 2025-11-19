using Auctions.Application.DTOs;
namespace Auctions.Application.Queries.GetBrandById;

public record GetBrandByIdQuery(Guid Id) : IQuery<BrandDto>;

