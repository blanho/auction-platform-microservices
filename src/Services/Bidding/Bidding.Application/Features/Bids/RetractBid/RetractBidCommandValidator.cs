using BuildingBlocks.Domain.Constants;

namespace Bidding.Application.Features.Bids.RetractBid;

public class RetractBidCommandValidator : AbstractValidator<RetractBidCommand>
{
    public RetractBidCommandValidator()
    {
        RuleFor(x => x.BidId)
            .NotEmpty()
            .WithMessage(ValidationConstants.Messages.Required("Bid ID"));

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage(ValidationConstants.Messages.Required("User ID"));

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage(ValidationConstants.Messages.Required("Reason for retraction"))
            .MaximumLength(ValidationConstants.StringLength.Reason)
            .WithMessage(ValidationConstants.Messages.MaxLength("Reason", ValidationConstants.StringLength.Reason));
    }
}
