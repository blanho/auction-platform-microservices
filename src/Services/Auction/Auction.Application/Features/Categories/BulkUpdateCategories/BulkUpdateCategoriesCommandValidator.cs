using FluentValidation;

namespace Auctions.Application.Commands.BulkUpdateCategories;

public class BulkUpdateCategoriesCommandValidator : AbstractValidator<BulkUpdateCategoriesCommand>
{
    public BulkUpdateCategoriesCommandValidator()
    {
        RuleFor(x => x.CategoryIds)
            .NotEmpty()
            .WithMessage("At least one category ID is required");

        RuleFor(x => x.CategoryIds)
            .Must(ids => ids.Count <= 100)
            .WithMessage("Cannot update more than 100 categories at once");

        RuleForEach(x => x.CategoryIds)
            .NotEmpty()
            .WithMessage("Category ID cannot be empty");
    }
}

