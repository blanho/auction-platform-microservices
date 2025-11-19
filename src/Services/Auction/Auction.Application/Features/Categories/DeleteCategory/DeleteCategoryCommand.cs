namespace Auctions.Application.Commands.DeleteCategory;

public record DeleteCategoryCommand(Guid Id) : ICommand<bool>;

