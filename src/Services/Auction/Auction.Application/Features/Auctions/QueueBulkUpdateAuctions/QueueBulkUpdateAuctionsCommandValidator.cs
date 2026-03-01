using FluentValidation;

namespace Auctions.Application.Commands.QueueBulkUpdateAuctions;

public class QueueBulkUpdateAuctionsCommandValidator : AbstractValidator<QueueBulkUpdateAuctionsCommand>
{
    public QueueBulkUpdateAuctionsCommandValidator()
    {
        RuleFor(x => x.RequestedBy).NotEmpty();
        RuleFor(x => x.AuctionIds).NotEmpty()
            .Must(ids => ids.Count <= 5000)
            .WithMessage("Bulk update cannot exceed 5,000 auctions per batch.");
    }
}
