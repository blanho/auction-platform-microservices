using BuildingBlocks.Domain.Constants;
using FluentValidation;

namespace Auctions.Application.Commands.ImportAuctions;

public class ImportAuctionsCommandValidator : AbstractValidator<ImportAuctionsCommand>
{
    public ImportAuctionsCommandValidator()
    {
        RuleFor(x => x.Auctions)
            .NotEmpty()
            .WithMessage(ValidationConstants.Messages.Required("Auctions"));

        RuleFor(x => x.Auctions)
            .Must(auctions => auctions.Count <= ValidationConstants.CollectionSize.MaxImportSize)
            .WithMessage($"Cannot import more than {ValidationConstants.CollectionSize.MaxImportSize} auctions at once");

        RuleFor(x => x.Seller)
            .NotEmpty()
            .WithMessage(ValidationConstants.Messages.Required("Seller"))
            .MaximumLength(ValidationConstants.StringLength.Username)
            .WithMessage(ValidationConstants.Messages.MaxLength("Seller", ValidationConstants.StringLength.Username));
    }
}

