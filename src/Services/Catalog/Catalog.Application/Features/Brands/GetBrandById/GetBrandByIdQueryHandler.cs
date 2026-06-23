using AutoMapper;
using Catalog.Application.Errors;

namespace Catalog.Application.Features.Brands.GetBrandById;

public class GetBrandByIdQueryHandler : IQueryHandler<GetBrandByIdQuery, BrandDto>
{
    private readonly IBrandRepository _brandRepository;
    private readonly IMapper _mapper;

    public GetBrandByIdQueryHandler(IBrandRepository brandRepository, IMapper mapper)
    {
        _brandRepository = brandRepository;
        _mapper = mapper;
    }

    public async Task<Result<BrandDto>> Handle(GetBrandByIdQuery request, CancellationToken cancellationToken)
    {
        var brand = await _brandRepository.GetByIdAsync(request.Id, cancellationToken);
        if (brand is null)
            return Result.Failure<BrandDto>(CatalogErrors.Brand.NotFound);

        return Result.Success(_mapper.Map<BrandDto>(brand));
    }
}
