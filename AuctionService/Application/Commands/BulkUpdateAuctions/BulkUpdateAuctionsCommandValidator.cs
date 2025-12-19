using FluentValidation;

namespace AuctionService.Application.Commands.BulkUpdateAuctions;

public class BulkUpdateAuctionsCommandValidator : AbstractValidator<BulkUpdateAuctionsCommand>
{
    public BulkUpdateAuctionsCommandValidator()
    {
        RuleFor(x => x.AuctionIds)
            .NotEmpty()
            .WithMessage("At least one auction ID is required");

        RuleFor(x => x.AuctionIds)
            .Must(ids => ids.Count <= 100)
            .WithMessage("Cannot update more than 100 auctions at once");

        RuleForEach(x => x.AuctionIds)
            .NotEmpty()
            .WithMessage("Auction ID cannot be empty");

        RuleFor(x => x.Reason)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Reason))
            .WithMessage("Reason must not exceed 500 characters");
    }
}
