using BuildingBlocks.Domain.Constants;
using FluentValidation;

namespace Auctions.Application.Features.Auctions.BulkUpdateAuctions;

public class BulkUpdateAuctionsCommandValidator : AbstractValidator<BulkUpdateAuctionsCommand>
{
    public BulkUpdateAuctionsCommandValidator()
    {
        RuleFor(x => x.AuctionIds)
            .NotEmpty()
            .WithMessage(ValidationConstants.Messages.Required("Auction IDs"));

        RuleFor(x => x.AuctionIds)
            .Must(ids => ids.Count <= ValidationConstants.CollectionSize.MaxBulkOperationSize)
            .WithMessage($"Cannot update more than {ValidationConstants.CollectionSize.MaxBulkOperationSize} auctions at once");

        RuleForEach(x => x.AuctionIds)
            .NotEmpty()
            .WithMessage(ValidationConstants.Messages.Required("Auction ID"));

        RuleFor(x => x.Reason)
            .MaximumLength(ValidationConstants.StringLength.Reason)
            .When(x => !string.IsNullOrEmpty(x.Reason))
            .WithMessage(ValidationConstants.Messages.MaxLength("Reason", ValidationConstants.StringLength.Reason));
    }
}

