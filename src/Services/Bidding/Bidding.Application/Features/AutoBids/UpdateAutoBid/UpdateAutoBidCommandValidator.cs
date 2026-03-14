using BuildingBlocks.Domain.Constants;

namespace Bidding.Application.Features.AutoBids.UpdateAutoBid;

public class UpdateAutoBidCommandValidator : AbstractValidator<UpdateAutoBidCommand>
{
    public UpdateAutoBidCommandValidator()
    {
        RuleFor(x => x.AutoBidId)
            .NotEmpty()
            .WithMessage(ValidationConstants.Messages.Required("Auto-bid ID"));

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage(ValidationConstants.Messages.Required("User ID"));

        RuleFor(x => x.NewMaxAmount)
            .GreaterThan(0)
            .WithMessage(ValidationConstants.Messages.MustBePositive("New max amount"))
            .LessThanOrEqualTo(10_000_000)
            .WithMessage(ValidationConstants.Messages.MustNotExceed("Max amount", 10_000_000));
    }
}
