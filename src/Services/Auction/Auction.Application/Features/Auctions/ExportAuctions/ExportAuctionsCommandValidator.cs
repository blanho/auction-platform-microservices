using BuildingBlocks.Domain.Constants;
using FluentValidation;

namespace Auctions.Application.Features.Auctions.ExportAuctions;

public class ExportAuctionsCommandValidator : AbstractValidator<ExportAuctionsCommand>
{
    public ExportAuctionsCommandValidator()
    {
        RuleFor(x => x.Format)
            .IsInEnum().WithMessage(ValidationConstants.Messages.InvalidEnumValue("Export format"));

        When(x => x.StartDate.HasValue && x.EndDate.HasValue, () =>
        {
            RuleFor(x => x)
                .Must(x => x.EndDate!.Value > x.StartDate!.Value)
                .WithMessage(ValidationConstants.Messages.Invalid("Date range"));
        });

        When(x => x.SellerFilter != null, () =>
        {
            RuleFor(x => x.SellerFilter)
                .MaximumLength(ValidationConstants.StringLength.Standard)
                .WithMessage(ValidationConstants.Messages.MaxLength("Seller filter", ValidationConstants.StringLength.Standard));
        });
    }
}
