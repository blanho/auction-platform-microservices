using BuildingBlocks.Domain.Constants;
using FluentValidation;

namespace Auctions.Application.Features.Categories.BulkUpdateCategories;

public class BulkUpdateCategoriesCommandValidator : AbstractValidator<BulkUpdateCategoriesCommand>
{
    public BulkUpdateCategoriesCommandValidator()
    {
        RuleFor(x => x.CategoryIds)
            .NotEmpty()
            .WithMessage(ValidationConstants.Messages.MustContainAtLeastOne("Category ID"));

        RuleFor(x => x.CategoryIds)
            .Must(ids => ids.Count <= ValidationConstants.CollectionSize.MaxBulkOperationSize)
            .WithMessage(ValidationConstants.Messages.MustNotExceed("Categories", ValidationConstants.CollectionSize.MaxBulkOperationSize));

        RuleForEach(x => x.CategoryIds)
            .NotEmpty()
            .WithMessage(ValidationConstants.Messages.Required("Category ID"));
    }
}

