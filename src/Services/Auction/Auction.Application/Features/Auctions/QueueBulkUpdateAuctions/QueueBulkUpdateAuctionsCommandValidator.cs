using BuildingBlocks.Domain.Constants;
using FluentValidation;

namespace Auctions.Application.Features.Auctions.QueueBulkUpdateAuctions;

public class QueueBulkUpdateAuctionsCommandValidator : AbstractValidator<QueueBulkUpdateAuctionsCommand>
{
    private const int MaxBulkUpdateSize = 5_000;

    public QueueBulkUpdateAuctionsCommandValidator()
    {
        RuleFor(x => x.RequestedBy)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("Requested by"));

        RuleFor(x => x.AuctionIds)
            .NotEmpty().WithMessage(ValidationConstants.Messages.MustContainAtLeastOne("Auction ID"))
            .Must(ids => ids.Count <= MaxBulkUpdateSize)
            .WithMessage(ValidationConstants.Messages.MustNotExceed("Auctions", MaxBulkUpdateSize));
    }
}
