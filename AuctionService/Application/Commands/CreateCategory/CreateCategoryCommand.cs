using AuctionService.Application.DTOs;
using Common.CQRS.Abstractions;

namespace AuctionService.Application.Commands.CreateCategory;

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
