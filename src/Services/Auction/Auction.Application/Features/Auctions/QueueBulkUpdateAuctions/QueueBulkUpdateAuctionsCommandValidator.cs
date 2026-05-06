using Auctions.Domain.Constants;
using BuildingBlocks.Domain.Constants;
using FluentValidation;

namespace Auctions.Application.Features.Auctions.QueueBulkUpdateAuctions;

public class QueueBulkUpdateAuctionsCommandValidator : AbstractValidator<QueueBulkUpdateAuctionsCommand>
{

    public QueueBulkUpdateAuctionsCommandValidator()
    {
        RuleFor(x => x.RequestedBy)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("Requested by"));

        RuleFor(x => x.AuctionIds)
            .NotEmpty().WithMessage(ValidationConstants.Messages.MustContainAtLeastOne("Auction ID"))
            .Must(ids => ids.Count <= AuctionDefaults.Batch.MaxBulkUpdateSize)
            .WithMessage(ValidationConstants.Messages.MustNotExceed("Auctions", AuctionDefaults.Batch.MaxBulkUpdateSize));
    }
}
