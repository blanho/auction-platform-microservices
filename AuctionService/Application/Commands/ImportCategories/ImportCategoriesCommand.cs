using AuctionService.Application.DTOs;
using Common.CQRS.Abstractions;

namespace AuctionService.Application.Commands.ImportCategories;

public record ImportCategoriesCommand(
    List<CreateCategoryDto> Categories
) : ICommand<ImportCategoriesResult>;

public record ImportCategoriesResult(
    int SuccessCount,
    int FailureCount,
    List<string> Errors
);
