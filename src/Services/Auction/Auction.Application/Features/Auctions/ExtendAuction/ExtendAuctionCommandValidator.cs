using BuildingBlocks.Domain.Constants;
using FluentValidation;

namespace Auctions.Application.Features.Auctions.ExtendAuction;

public class ExtendAuctionCommandValidator : AbstractValidator<ExtendAuctionCommand>
{
    private const int MinExtensionMinutes = 1;
    private const int MaxExtensionMinutes = 10080;

    public ExtendAuctionCommandValidator()
    {
        RuleFor(x => x.AuctionId)
            .NotEmpty()
            .WithMessage(ValidationConstants.Messages.Required("Auction ID"));

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage(ValidationConstants.Messages.Required("User ID"));

        RuleFor(x => x.ExtensionMinutes)
            .InclusiveBetween(MinExtensionMinutes, MaxExtensionMinutes)
            .WithMessage($"Extension must be between {MinExtensionMinutes} and {MaxExtensionMinutes} minutes");
    }
}
