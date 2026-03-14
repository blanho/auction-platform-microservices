namespace Auctions.Application.Features.Categories.DeleteCategory;

public record DeleteCategoryCommand(Guid Id) : ICommand<bool>;

