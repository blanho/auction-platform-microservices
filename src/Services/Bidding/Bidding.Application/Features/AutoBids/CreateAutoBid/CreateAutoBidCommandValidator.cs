using BuildingBlocks.Domain.Constants;

namespace Bidding.Application.Features.AutoBids.CreateAutoBid;

public class CreateAutoBidCommandValidator : AbstractValidator<CreateAutoBidCommand>
{
    public CreateAutoBidCommandValidator()
    {
        RuleFor(x => x.AuctionId)
            .NotEmpty()
            .WithMessage(ValidationConstants.Messages.Required("Auction ID"));

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage(ValidationConstants.Messages.Required("User ID"));

        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage(ValidationConstants.Messages.Required("Username"));

        RuleFor(x => x.MaxAmount)
            .GreaterThan(0)
            .WithMessage(ValidationConstants.Messages.MustBePositive("Max amount"))
            .LessThanOrEqualTo(10_000_000)
            .WithMessage(ValidationConstants.Messages.MustNotExceed("Max amount", 10_000_000));

        RuleFor(x => x.IncrementAmount)
            .GreaterThan(0)
            .When(x => x.IncrementAmount.HasValue)
            .WithMessage(ValidationConstants.Messages.MustBePositive("Increment amount"));
    }
}
