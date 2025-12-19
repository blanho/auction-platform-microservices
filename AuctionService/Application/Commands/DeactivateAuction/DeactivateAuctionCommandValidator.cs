using FluentValidation;

namespace AuctionService.Application.Commands.DeactivateAuction;

public class DeactivateAuctionCommandValidator : AbstractValidator<DeactivateAuctionCommand>
{
    public DeactivateAuctionCommandValidator()
    {
        RuleFor(x => x.AuctionId)
            .NotEmpty()
            .WithMessage("Auction ID is required");

        RuleFor(x => x.Reason)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Reason))
            .WithMessage("Reason must not exceed 500 characters");
    }
}
