using BuildingBlocks.Domain.Constants;
using FluentValidation;
using Payment.Application.DTOs;

namespace Payment.Application.Validators;

public class UpdateShippingDtoValidator : AbstractValidator<UpdateShippingDto>
{
    public UpdateShippingDtoValidator()
    {
        RuleFor(x => x.TrackingNumber)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("Tracking number"))
            .MaximumLength(ValidationConstants.StringLength.Standard)
            .WithMessage(ValidationConstants.Messages.MaxLength("Tracking number", ValidationConstants.StringLength.Standard));

        RuleFor(x => x.ShippingCarrier)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("Shipping carrier"))
            .MaximumLength(ValidationConstants.StringLength.Standard)
            .WithMessage(ValidationConstants.Messages.MaxLength("Shipping carrier", ValidationConstants.StringLength.Standard));

        When(x => !string.IsNullOrEmpty(x.SellerNotes), () =>
        {
            RuleFor(x => x.SellerNotes)
                .MaximumLength(ValidationConstants.StringLength.Extended)
                .WithMessage(ValidationConstants.Messages.MaxLength("Seller notes", ValidationConstants.StringLength.Extended));
        });
    }
}
