using BuildingBlocks.Domain.Constants;
using FluentValidation;
using Payment.Application.DTOs;

namespace Payment.Application.Validators;

public class CreateOrderDtoValidator : AbstractValidator<CreateOrderDto>
{
    public CreateOrderDtoValidator()
    {
        RuleFor(x => x.AuctionId)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("Auction ID"));

        When(x => x.BuyerUsername != null, () =>
        {
            RuleFor(x => x.BuyerUsername)
                .MaximumLength(256).WithMessage(ValidationConstants.Messages.MaxLength("Buyer username", 256));
        });

        When(x => x.SellerUsername != null, () =>
        {
            RuleFor(x => x.SellerUsername)
                .MaximumLength(256).WithMessage(ValidationConstants.Messages.MaxLength("Seller username", 256));
        });

        When(x => x.ItemTitle != null, () =>
        {
            RuleFor(x => x.ItemTitle)
                .MaximumLength(ValidationConstants.StringLength.Medium)
                .WithMessage(ValidationConstants.Messages.MaxLength("Item title", ValidationConstants.StringLength.Medium));
        });

        When(x => x.WinningBid.HasValue, () =>
        {
            RuleFor(x => x.WinningBid)
                .GreaterThan(0).WithMessage("Winning bid must be greater than 0");
        });

        When(x => x.ShippingCost.HasValue, () =>
        {
            RuleFor(x => x.ShippingCost)
                .GreaterThanOrEqualTo(0).WithMessage("Shipping cost must be non-negative");
        });

        When(x => x.PlatformFee.HasValue, () =>
        {
            RuleFor(x => x.PlatformFee)
                .GreaterThanOrEqualTo(0).WithMessage("Platform fee must be non-negative");
        });

        When(x => !string.IsNullOrEmpty(x.ShippingAddress), () =>
        {
            RuleFor(x => x.ShippingAddress)
                .MaximumLength(ValidationConstants.StringLength.Long)
                .WithMessage(ValidationConstants.Messages.MaxLength("Shipping address", ValidationConstants.StringLength.Long));
        });

        When(x => !string.IsNullOrEmpty(x.BuyerNotes), () =>
        {
            RuleFor(x => x.BuyerNotes)
                .MaximumLength(ValidationConstants.StringLength.Extended)
                .WithMessage(ValidationConstants.Messages.MaxLength("Buyer notes", ValidationConstants.StringLength.Extended));
        });
    }
}
