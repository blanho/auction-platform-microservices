namespace Catalog.Application.Features.Categories.GetCategoryTree;

public record GetCategoryTreeQuery(bool ActiveOnly = true) : IQuery<List<CategoryTreeDto>>;
