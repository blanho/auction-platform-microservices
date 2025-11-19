namespace Auctions.Application.Commands.BulkUpdateCategories;

public record BulkUpdateCategoriesCommand(
    List<Guid> CategoryIds,
    bool IsActive
) : ICommand<int>;

