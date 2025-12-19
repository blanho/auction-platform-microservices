using FluentValidation;

namespace AuctionService.Application.Commands.ImportAuctions;

public class ImportAuctionsCommandValidator : AbstractValidator<ImportAuctionsCommand>
{
    public ImportAuctionsCommandValidator()
    {
        RuleFor(x => x.Auctions)
            .NotEmpty()
            .WithMessage("At least one auction is required for import");

        RuleFor(x => x.Auctions)
            .Must(auctions => auctions.Count <= 1000)
            .WithMessage("Cannot import more than 1000 auctions at once");

        RuleFor(x => x.Seller)
            .NotEmpty()
            .WithMessage("Seller is required")
            .MaximumLength(256)
            .WithMessage("Seller must not exceed 256 characters");
    }
}
