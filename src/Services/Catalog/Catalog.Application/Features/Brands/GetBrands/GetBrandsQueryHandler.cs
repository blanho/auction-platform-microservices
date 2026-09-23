using AutoMapper;
using Catalog.Application.Filtering;

namespace Catalog.Application.Features.Brands.GetBrands;

public class GetBrandsQueryHandler : IQueryHandler<GetBrandsQuery, PaginatedResult<BrandDto>>
{
    private readonly IBrandRepository _brandRepository;
    private readonly IMapper _mapper;

    public GetBrandsQueryHandler(IBrandRepository brandRepository, IMapper mapper)
    {
        _brandRepository = brandRepository;
        _mapper = mapper;
    }

    public async Task<Result<PaginatedResult<BrandDto>>> Handle(GetBrandsQuery request, CancellationToken cancellationToken)
    {
        var parameters = new BrandQueryParams
        {
            ActiveOnly = request.ActiveOnly,
            FeaturedOnly = request.FeaturedOnly,
            Page = request.Page,
            PageSize = request.PageSize,
            Search = request.Search?.Trim(),
            SortBy = request.SortBy,
            SortDescending = string.Equals(request.SortOrder, "desc", StringComparison.OrdinalIgnoreCase)
        };
        var brands = await _brandRepository.GetPagedAsync(parameters, cancellationToken);
        return Result.Success(new PaginatedResult<BrandDto>(
            _mapper.Map<List<BrandDto>>(brands.Items), brands.TotalCount, brands.Page, brands.PageSize));
    }
}
