using Auctions.Domain.Constants;
using BuildingBlocks.Domain.Constants;
using FluentValidation;

namespace Auctions.Application.Features.Auctions.QueueAuctionImport;

public class QueueAuctionImportCommandValidator : AbstractValidator<QueueAuctionImportCommand>
{

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
            .Must(rows => rows.Count <= AuctionDefaults.Batch.MaxImportRowsPerRequest)
            .WithMessage(ValidationConstants.Messages.MustNotExceed("Rows", AuctionDefaults.Batch.MaxImportRowsPerRequest));
    }
}
