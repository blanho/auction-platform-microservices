using AutoMapper;
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
        var rootCategories = await _categoryRepository.GetRootCategoriesWithChildrenAsync(cancellationToken);
        var tree = MapToTree(rootCategories);
        return Result.Success(tree);
    }

    private static List<CategoryTreeDto> MapToTree(IEnumerable<Category> categories)
    {
        return categories.Select(c => new CategoryTreeDto
        {
            Id = c.Id,
            Name = c.Name,
            Slug = c.Slug,
            Icon = c.Icon,
            Children = MapToTree(c.SubCategories)
        }).ToList();
    }
}
