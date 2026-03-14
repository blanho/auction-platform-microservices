using BuildingBlocks.Domain.Constants;
using FluentValidation;

namespace Auctions.Application.Features.Auctions.QueueAuctionImport;

public class QueueAuctionImportCommandValidator : AbstractValidator<QueueAuctionImportCommand>
{
    private const int MaxRowsPerBatch = 10_000;

    public QueueAuctionImportCommandValidator()
    {
        RuleFor(x => x.SellerId)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("Seller ID"));

        RuleFor(x => x.SellerUsername)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("Seller username"));

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("Currency"));

        RuleFor(x => x.Rows)
            .NotEmpty().WithMessage(ValidationConstants.Messages.MustContainAtLeastOne("Row"))
            .Must(rows => rows.Count <= MaxRowsPerBatch)
            .WithMessage(ValidationConstants.Messages.MustNotExceed("Rows", MaxRowsPerBatch));
    }
}
