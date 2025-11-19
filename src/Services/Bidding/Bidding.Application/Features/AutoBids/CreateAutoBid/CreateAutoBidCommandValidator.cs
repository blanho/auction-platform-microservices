namespace Bidding.Application.Features.AutoBids.CreateAutoBid;

public class CreateAutoBidCommandValidator : AbstractValidator<CreateAutoBidCommand>
{
    public CreateAutoBidCommandValidator()
    {
        RuleFor(x => x.AuctionId)
            .NotEmpty()
            .WithMessage("Auction ID is required");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");

        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("Username is required");

        RuleFor(x => x.MaxAmount)
            .GreaterThan(0)
            .WithMessage("Max amount must be greater than zero")
            .LessThanOrEqualTo(10_000_000)
            .WithMessage("Max amount cannot exceed $10,000,000");

        RuleFor(x => x.IncrementAmount)
            .GreaterThan(0)
            .When(x => x.IncrementAmount.HasValue)
            .WithMessage("Increment amount must be greater than zero");
    }
}
