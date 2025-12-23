using FluentValidation;
using PaymentService.Application.DTOs;

namespace PaymentService.Application.Validators;

public class UpdateShippingDtoValidator : AbstractValidator<UpdateShippingDto>
{
    public UpdateShippingDtoValidator()
    {
        RuleFor(x => x.TrackingNumber)
            .NotEmpty().WithMessage("Tracking number is required")
            .MaximumLength(100).WithMessage("Tracking number must not exceed 100 characters");

        RuleFor(x => x.ShippingCarrier)
            .NotEmpty().WithMessage("Shipping carrier is required")
            .MaximumLength(100).WithMessage("Shipping carrier must not exceed 100 characters");

        When(x => !string.IsNullOrEmpty(x.SellerNotes), () =>
        {
            RuleFor(x => x.SellerNotes)
                .MaximumLength(1000).WithMessage("Seller notes must not exceed 1000 characters");
        });
    }
}
