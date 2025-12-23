using FluentValidation;

namespace PaymentService.Application.Commands.CreateOrder;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.AuctionId)
            .NotEmpty().WithMessage("Auction ID is required");

        RuleFor(x => x.BuyerId)
            .NotEmpty().WithMessage("Buyer ID is required");

        RuleFor(x => x.BuyerUsername)
            .NotEmpty().WithMessage("Buyer username is required")
            .MaximumLength(256).WithMessage("Buyer username must not exceed 256 characters");

        RuleFor(x => x.SellerId)
            .NotEmpty().WithMessage("Seller ID is required");

        RuleFor(x => x.SellerUsername)
            .NotEmpty().WithMessage("Seller username is required")
            .MaximumLength(256).WithMessage("Seller username must not exceed 256 characters");

        RuleFor(x => x.ItemTitle)
            .NotEmpty().WithMessage("Item title is required")
            .MaximumLength(200).WithMessage("Item title must not exceed 200 characters");

        RuleFor(x => x.WinningBid)
            .GreaterThan(0).WithMessage("Winning bid must be greater than 0");

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
                .MaximumLength(500).WithMessage("Shipping address must not exceed 500 characters");
        });

        When(x => !string.IsNullOrEmpty(x.BuyerNotes), () =>
        {
            RuleFor(x => x.BuyerNotes)
                .MaximumLength(1000).WithMessage("Buyer notes must not exceed 1000 characters");
        });
    }
}
