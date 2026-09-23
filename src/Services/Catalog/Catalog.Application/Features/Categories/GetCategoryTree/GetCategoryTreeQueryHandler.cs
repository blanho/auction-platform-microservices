using Catalog.Domain.Entities;

namespace Catalog.Application.Features.Categories.GetCategoryTree;

public class GetCategoryTreeQueryHandler : IQueryHandler<GetCategoryTreeQuery, List<CategoryTreeDto>>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetCategoryTreeQueryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<List<CategoryTreeDto>>> Handle(GetCategoryTreeQuery request, CancellationToken cancellationToken)
    {
        var categories = await _categoryRepository.GetAllAsync(!request.ActiveOnly, cancellationToken);
        var children = categories.ToLookup(c => c.ParentCategoryId);
        var tree = MapToTree(children, null);
        return Result.Success(tree);
    }

    private static List<CategoryTreeDto> MapToTree(ILookup<Guid?, Category> children, Guid? parentId)
    {
        return children[parentId].OrderBy(c => c.DisplayOrder).ThenBy(c => c.Name).Select(c => new CategoryTreeDto
        {
            Id = c.Id,
            Name = c.Name,
            Slug = c.Slug,
            Icon = c.Icon,
            Description = c.Description,
            DisplayOrder = c.DisplayOrder,
            IsActive = c.IsActive,
            ParentCategoryId = c.ParentCategoryId,
            Children = MapToTree(children, c.Id)
        }).ToList();
    }
}
