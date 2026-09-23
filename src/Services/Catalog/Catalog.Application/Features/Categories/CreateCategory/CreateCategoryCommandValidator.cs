using BuildingBlocks.Application.Helpers;
using FluentValidation;

namespace Catalog.Application.Features.Categories.CreateCategory;

public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => SlugHelper.GenerateSlug(x.Slug ?? x.Name)).NotEmpty().MaximumLength(100).OverridePropertyName("Slug");
        RuleFor(x => x.Icon).NotEmpty().MaximumLength(50);
    }
}
