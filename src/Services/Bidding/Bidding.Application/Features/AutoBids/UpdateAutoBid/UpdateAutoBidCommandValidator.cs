namespace Bidding.Application.Features.AutoBids.UpdateAutoBid;

public class UpdateAutoBidCommandValidator : AbstractValidator<UpdateAutoBidCommand>
{
    public UpdateAutoBidCommandValidator()
    {
        RuleFor(x => x.AutoBidId)
            .NotEmpty()
            .WithMessage("Auto-bid ID is required");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");

        RuleFor(x => x.NewMaxAmount)
            .GreaterThan(0)
            .WithMessage("New max amount must be greater than zero")
            .LessThanOrEqualTo(10_000_000)
            .WithMessage("Max amount cannot exceed $10,000,000");
    }
}
