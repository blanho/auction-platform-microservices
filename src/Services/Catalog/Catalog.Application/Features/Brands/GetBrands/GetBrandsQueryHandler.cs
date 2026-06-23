using AutoMapper;
using Catalog.Domain.Entities;

namespace Catalog.Application.Features.Brands.GetBrands;

public class GetBrandsQueryHandler : IQueryHandler<GetBrandsQuery, List<BrandDto>>
{
    private readonly IBrandRepository _brandRepository;
    private readonly IMapper _mapper;

    public GetBrandsQueryHandler(IBrandRepository brandRepository, IMapper mapper)
    {
        _brandRepository = brandRepository;
        _mapper = mapper;
    }

    public async Task<Result<List<BrandDto>>> Handle(GetBrandsQuery request, CancellationToken cancellationToken)
    {
        List<Brand> brands;

        if (request.FeaturedOnly && request.Count.HasValue)
            brands = await _brandRepository.GetFeaturedBrandsAsync(request.Count.Value, cancellationToken);
        else if (request.FeaturedOnly)
            brands = await _brandRepository.GetFeaturedBrandsAsync(cancellationToken: cancellationToken);
        else
            brands = await _brandRepository.GetAllAsync(!request.ActiveOnly, cancellationToken);

        return Result.Success(_mapper.Map<List<BrandDto>>(brands));
    }
}
