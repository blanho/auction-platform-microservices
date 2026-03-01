using FluentValidation;

namespace Auctions.Application.Commands.ImportAuctions;

public class ImportAuctionsCommandValidator : AbstractValidator<ImportAuctionsCommand>
{
    private const int MaxRowsPerImport = 10_000;

    public ImportAuctionsCommandValidator()
    {
        RuleFor(x => x.SellerId)
            .NotEmpty().WithMessage("SellerId is required.");

        RuleFor(x => x.SellerUsername)
            .NotEmpty().WithMessage("SellerUsername is required.");

        RuleFor(x => x.CorrelationId)
            .NotEmpty().WithMessage("CorrelationId is required for tracking the import.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .MaximumLength(3).WithMessage("Currency must be a 3-letter code.");

        RuleFor(x => x.Rows)
            .NotNull().WithMessage("Rows collection is required.")
            .Must(rows => rows.Count > 0).WithMessage("At least one row is required.")
            .Must(rows => rows.Count <= MaxRowsPerImport)
            .WithMessage($"Cannot import more than {MaxRowsPerImport:N0} rows in a single batch.");
    }
}
