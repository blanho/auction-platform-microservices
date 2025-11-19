using BuildingBlocks.Domain.Constants;
using FluentValidation;

namespace Auctions.Application.Commands.BuyNow;

public class BuyNowCommandValidator : AbstractValidator<BuyNowCommand>
{
    public BuyNowCommandValidator()
    {
        RuleFor(x => x.AuctionId)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("Auction ID"));

        RuleFor(x => x.BuyerId)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("Buyer ID"));

        RuleFor(x => x.BuyerUsername)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("Buyer username"))
            .MaximumLength(ValidationConstants.StringLength.Username)
            .WithMessage(ValidationConstants.Messages.MaxLength("Buyer username", ValidationConstants.StringLength.Username));
    }
}

