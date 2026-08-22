using BuildingBlocks.Domain.Constants;

namespace Payment.Application.Features.Orders.PrepareCheckout;

public class PrepareCheckoutCommandValidator : AbstractValidator<PrepareCheckoutCommand>
{
    public PrepareCheckoutCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("Order ID"));

        RuleFor(x => x.BuyerId)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("Buyer ID"));

        RuleFor(x => x.ShippingAddress)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("Shipping address"))
            .MaximumLength(ValidationConstants.StringLength.Long)
            .WithMessage(ValidationConstants.Messages.MaxLength(
                "Shipping address",
                ValidationConstants.StringLength.Long));

        When(x => !string.IsNullOrWhiteSpace(x.BuyerNotes), () =>
        {
            RuleFor(x => x.BuyerNotes)
                .MaximumLength(ValidationConstants.StringLength.Extended)
                .WithMessage(ValidationConstants.Messages.MaxLength(
                    "Buyer notes",
                    ValidationConstants.StringLength.Extended));
        });
    }
}
