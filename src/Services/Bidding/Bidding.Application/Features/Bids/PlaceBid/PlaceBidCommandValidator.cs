using BuildingBlocks.Domain.Constants;

namespace Bidding.Application.Features.Bids.PlaceBid;

public class PlaceBidCommandValidator : AbstractValidator<PlaceBidCommand>
{
    public PlaceBidCommandValidator()
    {
        RuleFor(x => x.AuctionId)
            .NotEmpty()
            .WithMessage(ValidationConstants.Messages.Required("Auction ID"));

        RuleFor(x => x.BidderId)
            .NotEmpty()
            .WithMessage(ValidationConstants.Messages.Required("Bidder ID"));

        RuleFor(x => x.BidderUsername)
            .NotEmpty()
            .WithMessage(ValidationConstants.Messages.Required("Bidder username"))
            .MaximumLength(ValidationConstants.StringLength.Username)
            .WithMessage(ValidationConstants.Messages.MaxLength("Bidder username", ValidationConstants.StringLength.Username));

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage(ValidationConstants.Messages.MustBePositive("Bid amount"))
            .LessThanOrEqualTo(BidDefaults.MaxBidAmount)
            .WithMessage(ValidationConstants.Messages.OutOfRange("Bid amount", 0, BidDefaults.MaxBidAmount));

        RuleFor(x => x.IdempotencyKey)
            .NotEmpty()
            .WithMessage(ValidationConstants.Messages.Required("Idempotency key"));
    }
}
