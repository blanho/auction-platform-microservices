namespace Catalog.Application.Features.Brands.GetBrandById;

public record GetBrandByIdQuery(Guid Id) : IQuery<BrandDto>;
