using Auctions.Domain.Constants;
using BuildingBlocks.Domain.Constants;
using FluentValidation;

namespace Auctions.Application.Features.Auctions.ImportAuctions;

public class ImportAuctionsCommandValidator : AbstractValidator<ImportAuctionsCommand>
{

    public ImportAuctionsCommandValidator()
    {
        RuleFor(x => x.SellerId)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("Seller ID"));

        RuleFor(x => x.SellerUsername)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("Seller username"));

        RuleFor(x => x.CorrelationId)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("Correlation ID"));

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("Currency"))
            .MaximumLength(ValidationConstants.MinLength.ShortCode + 1)
            .WithMessage(ValidationConstants.Messages.MaxLength("Currency", ValidationConstants.MinLength.ShortCode + 1));

        RuleFor(x => x.Rows)
            .NotNull().WithMessage(ValidationConstants.Messages.Required("Rows"))
            .Must(rows => rows.Count > 0).WithMessage(ValidationConstants.Messages.MustContainAtLeastOne("Row"))
            .Must(rows => rows.Count <= AuctionDefaults.Batch.MaxImportRowsPerRequest)
            .WithMessage(ValidationConstants.Messages.MustNotExceed("Rows", AuctionDefaults.Batch.MaxImportRowsPerRequest));
    }
}
