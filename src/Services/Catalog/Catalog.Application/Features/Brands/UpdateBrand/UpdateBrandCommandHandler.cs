using AutoMapper;
using Catalog.Application.Errors;

namespace Catalog.Application.Features.Brands.UpdateBrand;

public class UpdateBrandCommandHandler : ICommandHandler<UpdateBrandCommand, BrandDto>
{
    private readonly IBrandRepository _brandRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateBrandCommandHandler> _logger;

    public UpdateBrandCommandHandler(
        IBrandRepository brandRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<UpdateBrandCommandHandler> logger)
    {
        _brandRepository = brandRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<BrandDto>> Handle(UpdateBrandCommand request, CancellationToken cancellationToken)
    {
        var brand = await _brandRepository.GetByIdAsync(request.Id, cancellationToken);
        if (brand is null)
            return Result.Failure<BrandDto>(CatalogErrors.Brand.NotFound);

        if (request.Name is not null)
        {
            var slug = request.Name.ToLowerInvariant().Replace(" ", "-");
            var slugExists = await _brandRepository.SlugExistsAsync(slug, excludeId: request.Id, cancellationToken: cancellationToken);
            if (slugExists)
                return Result.Failure<BrandDto>(CatalogErrors.Brand.SlugAlreadyExists);
        }

        brand.Update(request.Name, null, request.Description, request.DisplayOrder, request.IsActive, request.IsFeatured);
        await _brandRepository.UpdateAsync(brand, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Brand {BrandId} updated", brand.Id);

        return Result.Success(_mapper.Map<BrandDto>(brand));
    }
}
