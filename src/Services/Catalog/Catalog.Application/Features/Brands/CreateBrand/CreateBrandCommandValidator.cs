using BuildingBlocks.Application.Helpers;
using FluentValidation;

namespace Catalog.Application.Features.Brands.CreateBrand;

public class CreateBrandCommandValidator : AbstractValidator<CreateBrandCommand>
{
    public CreateBrandCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => SlugHelper.GenerateSlug(x.Name)).NotEmpty().MaximumLength(100).OverridePropertyName("Slug");
    }
}
