using BuildingBlocks.Domain.Constants;
using FluentValidation;
using Payment.Application.DTOs;

namespace Payment.Application.Features.Orders.UpdateOrderStatus;

public class UpdateOrderDtoValidator : AbstractValidator<UpdateOrderDto>
{
    public UpdateOrderDtoValidator()
    {
        When(x => x.Status.HasValue, () =>
        {
            RuleFor(x => x.Status)
                .IsInEnum().WithMessage(ValidationConstants.Messages.InvalidEnumValue("Order status"));
        });

        When(x => x.PaymentStatus.HasValue, () =>
        {
            RuleFor(x => x.PaymentStatus)
                .IsInEnum().WithMessage(ValidationConstants.Messages.InvalidEnumValue("Payment status"));
        });

        When(x => !string.IsNullOrEmpty(x.PaymentTransactionId), () =>
        {
            RuleFor(x => x.PaymentTransactionId)
                .MaximumLength(ValidationConstants.StringLength.Username)
                .WithMessage(ValidationConstants.Messages.MaxLength("Payment transaction ID", ValidationConstants.StringLength.Username));
        });

        When(x => !string.IsNullOrEmpty(x.ShippingAddress), () =>
        {
            RuleFor(x => x.ShippingAddress)
                .MaximumLength(ValidationConstants.StringLength.Long)
                .WithMessage(ValidationConstants.Messages.MaxLength("Shipping address", ValidationConstants.StringLength.Long));
        });

        When(x => !string.IsNullOrEmpty(x.TrackingNumber), () =>
        {
            RuleFor(x => x.TrackingNumber)
                .MaximumLength(ValidationConstants.StringLength.Standard)
                .WithMessage(ValidationConstants.Messages.MaxLength("Tracking number", ValidationConstants.StringLength.Standard));
        });

        When(x => !string.IsNullOrEmpty(x.ShippingCarrier), () =>
        {
            RuleFor(x => x.ShippingCarrier)
                .MaximumLength(ValidationConstants.StringLength.Standard)
                .WithMessage(ValidationConstants.Messages.MaxLength("Shipping carrier", ValidationConstants.StringLength.Standard));
        });

        When(x => !string.IsNullOrEmpty(x.BuyerNotes), () =>
        {
            RuleFor(x => x.BuyerNotes)
                .MaximumLength(ValidationConstants.StringLength.Extended)
                .WithMessage(ValidationConstants.Messages.MaxLength("Buyer notes", ValidationConstants.StringLength.Extended));
        });

        When(x => !string.IsNullOrEmpty(x.SellerNotes), () =>
        {
            RuleFor(x => x.SellerNotes)
                .MaximumLength(ValidationConstants.StringLength.Extended)
                .WithMessage(ValidationConstants.Messages.MaxLength("Seller notes", ValidationConstants.StringLength.Extended));
        });
    }
}
