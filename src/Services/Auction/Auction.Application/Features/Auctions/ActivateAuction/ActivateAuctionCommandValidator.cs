using BuildingBlocks.Domain.Constants;
using FluentValidation;

namespace Auctions.Application.Features.Auctions.ActivateAuction;

public class ActivateAuctionCommandValidator : AbstractValidator<ActivateAuctionCommand>
{
    public ActivateAuctionCommandValidator()
    {
        RuleFor(x => x.AuctionId)
            .NotEmpty()
            .WithMessage(ValidationConstants.Messages.Required("Auction ID"));
    }
}

