using BuildingBlocks.Domain.Constants;
using FluentValidation;

namespace Auctions.Application.Commands.UpdateBrand;

public class UpdateBrandCommandValidator : AbstractValidator<UpdateBrandCommand>
{
    public UpdateBrandCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("Brand ID"));

        RuleFor(x => x.Name)
            .MaximumLength(ValidationConstants.StringLength.Standard)
            .WithMessage(ValidationConstants.Messages.MaxLength("Name", ValidationConstants.StringLength.Standard))
            .When(x => !string.IsNullOrWhiteSpace(x.Name));

        RuleFor(x => x.LogoUrl)
            .MaximumLength(ValidationConstants.StringLength.Long)
            .WithMessage(ValidationConstants.Messages.MaxLength("Logo URL", ValidationConstants.StringLength.Long))
            .When(x => x.LogoUrl != null);

        RuleFor(x => x.Description)
            .MaximumLength(ValidationConstants.StringLength.Extended)
            .WithMessage(ValidationConstants.Messages.MaxLength("Description", ValidationConstants.StringLength.Extended))
            .When(x => x.Description != null);

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Display order must be non-negative")
            .When(x => x.DisplayOrder.HasValue);
    }
}

