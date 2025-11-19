using FluentValidation;

namespace Auctions.Application.Commands.ImportCategories;

public class ImportCategoriesCommandValidator : AbstractValidator<ImportCategoriesCommand>
{
    public ImportCategoriesCommandValidator()
    {
        RuleFor(x => x.Categories)
            .NotEmpty()
            .WithMessage("At least one category is required for import");

        RuleFor(x => x.Categories)
            .Must(categories => categories.Count <= 500)
            .WithMessage("Cannot import more than 500 categories at once");
    }
}

