using AuctionService.Application.DTOs;
using Common.CQRS.Abstractions;

namespace AuctionService.Application.Commands.UpdateCategory;

public record UpdateCategoryCommand(
    Guid Id,
    string Name,
    string Slug,
    string Icon,
    string? Description,
    string? ImageUrl,
    int DisplayOrder,
    bool IsActive,
    Guid? ParentCategoryId
) : ICommand<CategoryDto>;
