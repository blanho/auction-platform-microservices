using FluentValidation;

namespace PaymentService.Application.Commands.UpdateOrderStatus;

public class UpdateOrderStatusCommandValidator : AbstractValidator<UpdateOrderStatusCommand>
{
    public UpdateOrderStatusCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required");

        When(x => x.Status.HasValue, () =>
        {
            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Invalid order status");
        });

        When(x => x.PaymentStatus.HasValue, () =>
        {
            RuleFor(x => x.PaymentStatus)
                .IsInEnum().WithMessage("Invalid payment status");
        });

        When(x => !string.IsNullOrEmpty(x.PaymentTransactionId), () =>
        {
            RuleFor(x => x.PaymentTransactionId)
                .MaximumLength(256).WithMessage("Payment transaction ID must not exceed 256 characters");
        });

        When(x => !string.IsNullOrEmpty(x.ShippingAddress), () =>
        {
            RuleFor(x => x.ShippingAddress)
                .MaximumLength(500).WithMessage("Shipping address must not exceed 500 characters");
        });

        When(x => !string.IsNullOrEmpty(x.TrackingNumber), () =>
        {
            RuleFor(x => x.TrackingNumber)
                .MaximumLength(100).WithMessage("Tracking number must not exceed 100 characters");
        });

        When(x => !string.IsNullOrEmpty(x.ShippingCarrier), () =>
        {
            RuleFor(x => x.ShippingCarrier)
                .MaximumLength(100).WithMessage("Shipping carrier must not exceed 100 characters");
        });

        When(x => !string.IsNullOrEmpty(x.BuyerNotes), () =>
        {
            RuleFor(x => x.BuyerNotes)
                .MaximumLength(1000).WithMessage("Buyer notes must not exceed 1000 characters");
        });

        When(x => !string.IsNullOrEmpty(x.SellerNotes), () =>
        {
            RuleFor(x => x.SellerNotes)
                .MaximumLength(1000).WithMessage("Seller notes must not exceed 1000 characters");
        });
    }
}
