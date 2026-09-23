using AutoMapper;
using BuildingBlocks.Application.Helpers;
using Catalog.Application.Errors;

namespace Catalog.Application.Features.Categories.UpdateCategory;

public class UpdateCategoryCommandHandler : ICommandHandler<UpdateCategoryCommand, CategoryDto>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateCategoryCommandHandler> _logger;

    public UpdateCategoryCommandHandler(
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<UpdateCategoryCommandHandler> logger)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<CategoryDto>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(request.Id, cancellationToken);
        if (category is null)
            return Result.Failure<CategoryDto>(CatalogErrors.Category.NotFound);

        if (category.IsActive && !request.IsActive && await _categoryRepository.HasChildrenAsync(request.Id, cancellationToken))
            return Result.Failure<CategoryDto>(CatalogErrors.Category.HasChildren);

        var visited = new HashSet<Guid> { request.Id };
        var parentId = request.ParentCategoryId;
        while (parentId.HasValue)
        {
            if (!visited.Add(parentId.Value))
                return Result.Failure<CategoryDto>(CatalogErrors.Category.CannotBeOwnParent);

            var parent = await _categoryRepository.GetByIdAsync(parentId.Value, cancellationToken);
            if (parent is null)
                return Result.Failure<CategoryDto>(CatalogErrors.Category.ParentNotFound);

            if (request.IsActive && !parent.IsActive)
                return Result.Failure<CategoryDto>(CatalogErrors.Category.ParentInactive);

            parentId = parent.ParentCategoryId;
        }

        var slug = SlugHelper.GenerateSlug(request.Slug ?? request.Name);
        var slugExists = await _categoryRepository.SlugExistsAsync(slug, excludeId: request.Id, cancellationToken: cancellationToken);
        if (slugExists)
            return Result.Failure<CategoryDto>(CatalogErrors.Category.SlugAlreadyExists);

        category.Update(request.Name, slug, request.Icon, request.Description, request.DisplayOrder, request.IsActive, request.ParentCategoryId);
        await _categoryRepository.UpdateAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Category {CategoryId} updated", category.Id);

        return Result.Success(_mapper.Map<CategoryDto>(category));
    }
}
