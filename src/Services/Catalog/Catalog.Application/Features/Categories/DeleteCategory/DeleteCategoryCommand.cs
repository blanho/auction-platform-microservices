namespace Catalog.Application.Features.Categories.DeleteCategory;

public record DeleteCategoryCommand(Guid Id) : ICommand;
