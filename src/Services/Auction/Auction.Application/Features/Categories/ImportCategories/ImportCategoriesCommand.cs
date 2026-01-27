using Auctions.Application.DTOs;
namespace Auctions.Application.Features.Categories.ImportCategories;

public record ImportCategoriesCommand(
    List<CreateCategoryDto> Categories
) : ICommand<ImportCategoriesResult>;

public record ImportCategoriesResult(
    int SuccessCount,
    int FailureCount,
    List<string> Errors
);

