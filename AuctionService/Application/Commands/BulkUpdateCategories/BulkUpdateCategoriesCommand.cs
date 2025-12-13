using Common.CQRS.Abstractions;

namespace AuctionService.Application.Commands.BulkUpdateCategories;

public record BulkUpdateCategoriesCommand(
    List<Guid> CategoryIds,
    bool IsActive
) : ICommand<int>;
