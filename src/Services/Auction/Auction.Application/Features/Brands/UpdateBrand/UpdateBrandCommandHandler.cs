using Auctions.Application.Errors;
using Auctions.Application.DTOs;
using AutoMapper;
using BuildingBlocks.Application.Helpers;
using Microsoft.Extensions.Logging;
// using BuildingBlocks.Infrastructure.Caching; // Use BuildingBlocks.Application.Abstractions instead
// using BuildingBlocks.Infrastructure.Repository; // Use BuildingBlocks.Application.Abstractions instead
namespace Auctions.Application.Features.Brands.UpdateBrand;

public class UpdateBrandCommandHandler : ICommandHandler<UpdateBrandCommand, BrandDto>
{
    private readonly IBrandRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateBrandCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateBrandCommandHandler(
        IBrandRepository repository,
        IMapper mapper,
        ILogger<UpdateBrandCommandHandler> logger,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BrandDto>> Handle(UpdateBrandCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating brand {BrandId}", request.Id);

        try
        {
            var brand = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (brand == null)
            {
                return Result.Failure<BrandDto>(AuctionErrors.Brand.NotFoundById(request.Id));
            }

            if (!string.IsNullOrWhiteSpace(request.Name) && request.Name != brand.Name)
            {
                var newSlug = SlugHelper.GenerateSlug(request.Name);
                var existingBrand = await _repository.GetBySlugAsync(newSlug, cancellationToken);
                if (existingBrand != null && existingBrand.Id != request.Id)
                {
                    return Result.Failure<BrandDto>(AuctionErrors.Brand.SlugExists(newSlug));
                }
                brand.Name = request.Name;
                brand.Slug = newSlug;
            }

            if (request.Description != null)
                brand.Description = request.Description;

            if (request.DisplayOrder.HasValue)
                brand.DisplayOrder = request.DisplayOrder.Value;

            if (request.IsActive.HasValue)
                brand.IsActive = request.IsActive.Value;

            if (request.IsFeatured.HasValue)
                brand.IsFeatured = request.IsFeatured.Value;

            await _repository.UpdateAsync(brand, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Updated brand {BrandId}", request.Id);

            var dto = _mapper.Map<BrandDto>(brand);
            return Result<BrandDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to update brand {BrandId}: {Error}", request.Id, ex.Message);
            return Result.Failure<BrandDto>(AuctionErrors.Brand.UpdateFailed(ex.Message));
        }
    }
}

