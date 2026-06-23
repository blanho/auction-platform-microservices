using AutoMapper;
using Catalog.Application.Errors;
using Catalog.Domain.Entities;

namespace Catalog.Application.Features.Categories.CreateCategory;

public class CreateCategoryCommandHandler : ICommandHandler<CreateCategoryCommand, CategoryDto>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateCategoryCommandHandler> _logger;

    public CreateCategoryCommandHandler(
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CreateCategoryCommandHandler> logger)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<CategoryDto>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var slug = (request.Slug ?? request.Name).ToLowerInvariant().Replace(" ", "-");

        var slugExists = await _categoryRepository.SlugExistsAsync(slug, cancellationToken: cancellationToken);
        if (slugExists)
            return Result.Failure<CategoryDto>(CatalogErrors.Category.SlugAlreadyExists);

        if (request.ParentCategoryId.HasValue)
        {
            var parentExists = await _categoryRepository.ExistsAsync(request.ParentCategoryId.Value, cancellationToken);
            if (!parentExists)
                return Result.Failure<CategoryDto>(CatalogErrors.Category.ParentNotFound);
        }

        var category = Category.Create(
            request.Name,
            slug,
            request.Icon,
            request.Description,
            request.DisplayOrder,
            true,
            request.ParentCategoryId);

        await _categoryRepository.CreateAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Category {CategoryName} (Id: {CategoryId}) created", category.Name, category.Id);

        return Result.Success(_mapper.Map<CategoryDto>(category));
    }
}
