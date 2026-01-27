using Auctions.Application.DTOs;
namespace Auctions.Application.Features.Categories.CreateCategory;

public record CreateCategoryCommand(
    string Name,
    string Slug,
    string Icon,
    string? Description,
    string? ImageUrl,
    int DisplayOrder,
    bool IsActive,
    Guid? ParentCategoryId
) : ICommand<CategoryDto>;

