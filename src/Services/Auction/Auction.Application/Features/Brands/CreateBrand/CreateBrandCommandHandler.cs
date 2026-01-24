using Auctions.Application.DTOs;
using Auctions.Domain.Entities;
using AutoMapper;
using Microsoft.Extensions.Logging;
using BuildingBlocks.Infrastructure.Caching;
using BuildingBlocks.Infrastructure.Repository;
using BuildingBlocks.Infrastructure.Repository.Specifications;
namespace Auctions.Application.Commands.CreateBrand;

public class CreateBrandCommandHandler : ICommandHandler<CreateBrandCommand, BrandDto>
{
    private readonly IBrandRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateBrandCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public CreateBrandCommandHandler(
        IBrandRepository repository,
        IMapper mapper,
        ILogger<CreateBrandCommandHandler> logger,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BrandDto>> Handle(CreateBrandCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating brand {Name}", request.Name);

        try
        {
            var slug = SlugHelper.GenerateSlug(request.Name);
            
            var existingBrand = await _repository.GetBySlugAsync(slug, cancellationToken);
            if (existingBrand != null)
            {
                return Result.Failure<BrandDto>(Error.Create("Brand.SlugExists", $"A brand with slug '{slug}' already exists"));
            }

            var brand = new Brand
            {
                Name = request.Name,
                Slug = slug,
                LogoUrl = request.LogoUrl,
                Description = request.Description,
                DisplayOrder = request.DisplayOrder,
                IsFeatured = request.IsFeatured,
                IsActive = true
            };

            var createdBrand = await _repository.AddAsync(brand, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created brand {BrandId} with name {Name}", createdBrand.Id, request.Name);

            var dto = _mapper.Map<BrandDto>(createdBrand);
            return Result<BrandDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to create brand {Name}: {Error}", request.Name, ex.Message);
            return Result.Failure<BrandDto>(Error.Create("Brand.CreateFailed", $"Failed to create brand: {ex.Message}"));
        }
    }
}

