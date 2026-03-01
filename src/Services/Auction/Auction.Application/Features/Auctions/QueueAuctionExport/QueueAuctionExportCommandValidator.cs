using FluentValidation;

namespace Auctions.Application.Commands.QueueAuctionExport;

public class QueueAuctionExportCommandValidator : AbstractValidator<QueueAuctionExportCommand>
{
    public QueueAuctionExportCommandValidator()
    {
        RuleFor(x => x.RequestedBy).NotEmpty();
        RuleFor(x => x.Format).IsInEnum();
    }
}
