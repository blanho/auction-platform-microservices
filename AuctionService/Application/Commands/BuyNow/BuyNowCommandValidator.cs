using FluentValidation;

namespace AuctionService.Application.Commands.BuyNow;

public class BuyNowCommandValidator : AbstractValidator<BuyNowCommand>
{
    public BuyNowCommandValidator()
    {
        RuleFor(x => x.AuctionId)
            .NotEmpty().WithMessage("Auction ID is required");

        RuleFor(x => x.BuyerId)
            .NotEmpty().WithMessage("Buyer ID is required");

        RuleFor(x => x.BuyerUsername)
            .NotEmpty().WithMessage("Buyer username is required")
            .MaximumLength(256).WithMessage("Buyer username must not exceed 256 characters");
    }
}
