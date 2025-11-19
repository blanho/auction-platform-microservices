namespace Bidding.Application.Features.Bids.RetractBid;

public class RetractBidCommandValidator : AbstractValidator<RetractBidCommand>
{
    public RetractBidCommandValidator()
    {
        RuleFor(x => x.BidId)
            .NotEmpty()
            .WithMessage("Bid ID is required");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Reason for retraction is required")
            .MaximumLength(500)
            .WithMessage("Reason cannot exceed 500 characters");
    }
}
