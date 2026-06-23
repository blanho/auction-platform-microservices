namespace Catalog.Application.Features.Categories.GetCategoryById;

public record GetCategoryByIdQuery(Guid Id) : IQuery<CategoryDto>;
