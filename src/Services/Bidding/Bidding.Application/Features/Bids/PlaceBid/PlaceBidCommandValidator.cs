namespace Bidding.Application.Features.Bids.Commands.PlaceBid;

public class PlaceBidCommandValidator : AbstractValidator<PlaceBidCommand>
{
    public PlaceBidCommandValidator()
    {
        RuleFor(x => x.AuctionId)
            .NotEmpty()
            .WithMessage("Auction ID is required");

        RuleFor(x => x.BidderId)
            .NotEmpty()
            .WithMessage("Bidder ID is required");

        RuleFor(x => x.BidderUsername)
            .NotEmpty()
            .WithMessage("Bidder username is required")
            .MaximumLength(256)
            .WithMessage("Bidder username cannot exceed 256 characters");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Bid amount must be greater than 0")
            .LessThanOrEqualTo(10_000_000)
            .WithMessage("Bid amount cannot exceed 10 million");

        RuleFor(x => x.IdempotencyKey)
            .NotEmpty()
            .WithMessage("Idempotency key is required");
    }
}
