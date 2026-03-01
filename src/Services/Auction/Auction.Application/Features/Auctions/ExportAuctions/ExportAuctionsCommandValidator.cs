using FluentValidation;

namespace Auctions.Application.Commands.ExportAuctions;

public class ExportAuctionsCommandValidator : AbstractValidator<ExportAuctionsCommand>
{
    public ExportAuctionsCommandValidator()
    {
        RuleFor(x => x.Format)
            .IsInEnum().WithMessage("Invalid export format specified.");

        When(x => x.StartDate.HasValue && x.EndDate.HasValue, () =>
        {
            RuleFor(x => x)
                .Must(x => x.EndDate!.Value > x.StartDate!.Value)
                .WithMessage("End date must be after start date.");
        });

        When(x => x.SellerFilter != null, () =>
        {
            RuleFor(x => x.SellerFilter)
                .MaximumLength(100).WithMessage("Seller filter must not exceed 100 characters.");
        });
    }
}
