namespace Catalog.Application.Features.Categories.GetCategories;

public record GetCategoriesQuery(bool ActiveOnly = true) : IQuery<List<CategoryDto>>;
