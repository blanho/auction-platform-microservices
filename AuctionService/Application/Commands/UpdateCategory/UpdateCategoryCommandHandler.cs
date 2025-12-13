using AuctionService.Application.DTOs;
using AuctionService.Application.Interfaces;
using AutoMapper;
using Common.Core.Helpers;
using Common.Core.Interfaces;
using Common.CQRS.Abstractions;
using Common.Repository.Interfaces;

namespace AuctionService.Application.Commands.UpdateCategory;

public class UpdateCategoryCommandHandler : ICommandHandler<UpdateCategoryCommand, CategoryDto>
{
    private readonly ICategoryRepository _repository;
    private readonly IMapper _mapper;
    private readonly IAppLogger<UpdateCategoryCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCategoryCommandHandler(
        ICategoryRepository repository,
        IMapper mapper,
        IAppLogger<UpdateCategoryCommandHandler> logger,
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

            var slugExists = await _repository.SlugExistsAsync(request.Slug, request.Id, cancellationToken);
            if (slugExists)
            {
                return Result.Failure<CategoryDto>(Error.Create("Category.SlugExists", $"A category with slug '{request.Slug}' already exists"));
            }

            if (request.ParentCategoryId == request.Id)
            {
                return Result.Failure<CategoryDto>(Error.Create("Category.SelfParent", "A category cannot be its own parent"));
            }

            category.Name = request.Name;
            category.Slug = request.Slug;
            category.Icon = request.Icon;
            category.Description = request.Description;
            category.ImageUrl = request.ImageUrl;
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
            return Result.Failure<CategoryDto>(Error.Create("Category.NotFound", $"Category with ID '{request.Id}' not found"));
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to update category {CategoryId}: {Error}", request.Id, ex.Message);
            return Result.Failure<CategoryDto>(Error.Create("Category.UpdateFailed", $"Failed to update category: {ex.Message}"));
        }
    }
}
