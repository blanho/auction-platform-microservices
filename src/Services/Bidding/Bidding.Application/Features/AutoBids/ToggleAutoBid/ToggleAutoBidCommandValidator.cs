using BuildingBlocks.Domain.Constants;

namespace Bidding.Application.Features.AutoBids.ToggleAutoBid;

public class ToggleAutoBidCommandValidator : AbstractValidator<ToggleAutoBidCommand>
{
    public ToggleAutoBidCommandValidator()
    {
        RuleFor(x => x.AutoBidId)
            .NotEmpty()
            .WithMessage(ValidationConstants.Messages.Required("Auto-bid ID"));

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage(ValidationConstants.Messages.Required("User ID"));
    }
}
