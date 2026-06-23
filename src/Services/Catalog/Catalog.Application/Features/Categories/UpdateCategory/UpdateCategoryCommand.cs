namespace Catalog.Application.Features.Categories.UpdateCategory;

public record UpdateCategoryCommand(
    Guid Id,
    string Name,
    string? Slug,
    string Icon,
    string? Description,
    int DisplayOrder,
    bool IsActive,
    Guid? ParentCategoryId) : ICommand<CategoryDto>;
