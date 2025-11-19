using BuildingBlocks.Domain.Constants;
using FluentValidation;

namespace Auctions.Application.Commands.DeactivateAuction;

public class DeactivateAuctionCommandValidator : AbstractValidator<DeactivateAuctionCommand>
{
    public DeactivateAuctionCommandValidator()
    {
        RuleFor(x => x.AuctionId)
            .NotEmpty()
            .WithMessage(ValidationConstants.Messages.Required("Auction ID"));

        RuleFor(x => x.Reason)
            .MaximumLength(ValidationConstants.StringLength.Reason)
            .When(x => !string.IsNullOrEmpty(x.Reason))
            .WithMessage(ValidationConstants.Messages.MaxLength("Reason", ValidationConstants.StringLength.Reason));
    }
}

