using BuildingBlocks.Domain.Constants;
using FluentValidation;

namespace Auctions.Application.Features.Auctions.CancelAuction;

public class CancelAuctionCommandValidator : AbstractValidator<CancelAuctionCommand>
{
    public CancelAuctionCommandValidator()
    {
        RuleFor(x => x.AuctionId)
            .NotEmpty()
            .WithMessage(ValidationConstants.Messages.Required("Auction ID"));

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage(ValidationConstants.Messages.Required("User ID"));

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage(ValidationConstants.Messages.Required("Cancellation reason"))
            .MaximumLength(500)
            .WithMessage(ValidationConstants.Messages.MaxLength("Cancellation reason", 500));
    }
}
