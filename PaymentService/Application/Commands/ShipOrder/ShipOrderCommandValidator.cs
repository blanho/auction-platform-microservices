using FluentValidation;

namespace PaymentService.Application.Commands.ShipOrder;

public class ShipOrderCommandValidator : AbstractValidator<ShipOrderCommand>
{
    public ShipOrderCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required");

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
