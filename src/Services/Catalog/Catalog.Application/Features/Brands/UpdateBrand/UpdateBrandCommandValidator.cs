using BuildingBlocks.Application.Helpers;
using FluentValidation;

namespace Catalog.Application.Features.Brands.UpdateBrand;

public class UpdateBrandCommandValidator : AbstractValidator<UpdateBrandCommand>
{
    public UpdateBrandCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100).When(x => x.Name is not null);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => SlugHelper.GenerateSlug(x.Name!)).NotEmpty().MaximumLength(100).OverridePropertyName("Slug").When(x => x.Name is not null);
    }
}
