using FluentValidation;

namespace Auctions.Application.Commands.ActivateAuction;

public class ActivateAuctionCommandValidator : AbstractValidator<ActivateAuctionCommand>
{
    public ActivateAuctionCommandValidator()
    {
        RuleFor(x => x.AuctionId)
            .NotEmpty()
            .WithMessage("Auction ID is required");
    }
}

