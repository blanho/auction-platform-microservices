using FluentValidation;

namespace Auctions.Application.Commands.QueueAuctionImport;

public class QueueAuctionImportCommandValidator : AbstractValidator<QueueAuctionImportCommand>
{
    public QueueAuctionImportCommandValidator()
    {
        RuleFor(x => x.SellerId).NotEmpty();
        RuleFor(x => x.SellerUsername).NotEmpty();
        RuleFor(x => x.Currency).NotEmpty();
        RuleFor(x => x.Rows).NotEmpty()
            .Must(rows => rows.Count <= 10000)
            .WithMessage("Import cannot exceed 10,000 rows per batch.");
    }
}
