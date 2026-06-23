using AutoMapper;
using Catalog.Application.Errors;
using Catalog.Domain.Entities;

namespace Catalog.Application.Features.Brands.CreateBrand;

public class CreateBrandCommandHandler : ICommandHandler<CreateBrandCommand, BrandDto>
{
    private readonly IBrandRepository _brandRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateBrandCommandHandler> _logger;

    public CreateBrandCommandHandler(
        IBrandRepository brandRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CreateBrandCommandHandler> logger)
    {
        _brandRepository = brandRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<BrandDto>> Handle(CreateBrandCommand request, CancellationToken cancellationToken)
    {
        var slug = request.Name.ToLowerInvariant().Replace(" ", "-");

        var slugExists = await _brandRepository.SlugExistsAsync(slug, cancellationToken: cancellationToken);
        if (slugExists)
            return Result.Failure<BrandDto>(CatalogErrors.Brand.SlugAlreadyExists);

        var brand = Brand.Create(
            request.Name,
            slug,
            request.Description,
            request.DisplayOrder,
            request.IsFeatured);

        await _brandRepository.AddAsync(brand, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Brand {BrandName} (Id: {BrandId}) created", brand.Name, brand.Id);

        return Result.Success(_mapper.Map<BrandDto>(brand));
    }
}
