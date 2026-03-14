using BuildingBlocks.Domain.Constants;
using FluentValidation;
using Payment.Application.DTOs;

namespace Payment.Application.Features.Orders.CreateOrder;

public class CreateOrderDtoValidator : AbstractValidator<CreateOrderDto>
{
    public CreateOrderDtoValidator()
    {
        RuleFor(x => x.AuctionId)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("Auction ID"));

        RuleFor(x => x.BuyerId)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("Buyer ID"));

        RuleFor(x => x.BuyerUsername)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("Buyer username"))
            .MaximumLength(ValidationConstants.StringLength.Username)
            .WithMessage(ValidationConstants.Messages.MaxLength("Buyer username", ValidationConstants.StringLength.Username));

        RuleFor(x => x.SellerId)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("Seller ID"));

        RuleFor(x => x.SellerUsername)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("Seller username"))
            .MaximumLength(ValidationConstants.StringLength.Username)
            .WithMessage(ValidationConstants.Messages.MaxLength("Seller username", ValidationConstants.StringLength.Username));

        RuleFor(x => x.ItemTitle)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("Item title"))
            .MaximumLength(ValidationConstants.StringLength.Medium)
            .WithMessage(ValidationConstants.Messages.MaxLength("Item title", ValidationConstants.StringLength.Medium));

        RuleFor(x => x.WinningBid)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("Winning bid"))
            .GreaterThan(0).WithMessage(ValidationConstants.Messages.MustBePositive("Winning bid"));

        When(x => x.ShippingCost.HasValue, () =>
        {
            RuleFor(x => x.ShippingCost)
                .GreaterThanOrEqualTo(0).WithMessage(ValidationConstants.Messages.MustBeNonNegative("Shipping cost"));
        });

        When(x => x.PlatformFee.HasValue, () =>
        {
            RuleFor(x => x.PlatformFee)
                .GreaterThanOrEqualTo(0).WithMessage(ValidationConstants.Messages.MustBeNonNegative("Platform fee"));
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
