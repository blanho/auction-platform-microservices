namespace Catalog.Application.Features.Categories.CreateCategory;

public record CreateCategoryCommand(
    string Name,
    string? Slug,
    string Icon,
    string? Description,
    int DisplayOrder,
    Guid? ParentCategoryId) : ICommand<CategoryDto>;
