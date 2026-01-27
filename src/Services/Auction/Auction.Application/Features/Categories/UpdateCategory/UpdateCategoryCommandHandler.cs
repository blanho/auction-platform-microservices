using Auctions.Application.Errors;
using Auctions.Application.DTOs;
using AutoMapper;
using Microsoft.Extensions.Logging;
using BuildingBlocks.Infrastructure.Caching;
using BuildingBlocks.Infrastructure.Repository;

namespace Auctions.Application.Features.Categories.UpdateCategory;

public class UpdateCategoryCommandHandler : ICommandHandler<UpdateCategoryCommand, CategoryDto>
{
    private readonly ICategoryRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateCategoryCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCategoryCommandHandler(
        ICategoryRepository repository,
        IMapper mapper,
        ILogger<UpdateCategoryCommandHandler> logger,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CategoryDto>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating category {CategoryId}", request.Id);

        try
        {
            var category = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (category == null)
            {
                return Result.Failure<CategoryDto>(AuctionErrors.Category.NotFoundById(request.Id));
            }

            var slugExists = await _repository.SlugExistsAsync(request.Slug, request.Id, cancellationToken);
            if (slugExists)
            {
                return Result.Failure<CategoryDto>(AuctionErrors.Category.SlugExists(request.Slug));
            }

            if (request.ParentCategoryId == request.Id)
            {
                return Result.Failure<CategoryDto>(AuctionErrors.Category.SelfParent);
            }

            category.Name = request.Name;
            category.Slug = request.Slug;
            category.Icon = request.Icon;
            category.Description = request.Description;
            category.DisplayOrder = request.DisplayOrder;
            category.IsActive = request.IsActive;
            category.ParentCategoryId = request.ParentCategoryId;

            await _repository.UpdateAsync(category, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Updated category {CategoryId}", request.Id);

            var dto = _mapper.Map<CategoryDto>(category);
            return Result<CategoryDto>.Success(dto);
        }
        catch (KeyNotFoundException)
        {
            return Result.Failure<CategoryDto>(AuctionErrors.Category.NotFoundById(request.Id));
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to update category {CategoryId}: {Error}", request.Id, ex.Message);
            return Result.Failure<CategoryDto>(AuctionErrors.Category.UpdateFailed(ex.Message));
        }
    }
}

