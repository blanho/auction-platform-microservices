using BuildingBlocks.Domain.Constants;
using FluentValidation;

namespace Auctions.Application.Features.Brands.CreateBrand;

public class CreateBrandCommandValidator : AbstractValidator<CreateBrandCommand>
{
    public CreateBrandCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("Name"))
            .MaximumLength(ValidationConstants.StringLength.Standard)
            .WithMessage(ValidationConstants.Messages.MaxLength("Name", ValidationConstants.StringLength.Standard));

        RuleFor(x => x.LogoUrl)
            .MaximumLength(ValidationConstants.StringLength.Long)
            .WithMessage(ValidationConstants.Messages.MaxLength("Logo URL", ValidationConstants.StringLength.Long));

        RuleFor(x => x.Description)
            .MaximumLength(ValidationConstants.StringLength.Extended)
            .WithMessage(ValidationConstants.Messages.MaxLength("Description", ValidationConstants.StringLength.Extended));

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Display order must be non-negative");
    }
}

