namespace Auctions.Application.Features.Categories.BulkUpdateCategories;

public record BulkUpdateCategoriesCommand(
    List<Guid> CategoryIds,
    bool IsActive
) : ICommand<int>;

