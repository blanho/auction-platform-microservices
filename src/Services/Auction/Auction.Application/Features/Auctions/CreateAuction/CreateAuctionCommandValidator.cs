using BuildingBlocks.Domain.Constants;
using FluentValidation;

namespace Auctions.Application.Commands.CreateAuction;

public class CreateAuctionCommandValidator : AbstractValidator<CreateAuctionCommand>
{
    public CreateAuctionCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("Title"))
            .MaximumLength(ValidationConstants.StringLength.Medium)
            .WithMessage(ValidationConstants.Messages.MaxLength("Title", ValidationConstants.StringLength.Medium))
            .MinimumLength(ValidationConstants.MinLength.Name)
            .WithMessage(ValidationConstants.Messages.MinLength("Title", ValidationConstants.MinLength.Name));

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("Description"))
            .MaximumLength(ValidationConstants.StringLength.Large)
            .WithMessage(ValidationConstants.Messages.MaxLength("Description", ValidationConstants.StringLength.Large))
            .MinimumLength(10).WithMessage(ValidationConstants.Messages.MinLength("Description", 10));

        When(x => x.Condition != null, () =>
        {
            RuleFor(x => x.Condition)
                .MaximumLength(ValidationConstants.StringLength.Short)
                .WithMessage(ValidationConstants.Messages.MaxLength("Condition", ValidationConstants.StringLength.Short));
        });

        When(x => x.YearManufactured != null, () =>
        {
            RuleFor(x => x.YearManufactured)
                .InclusiveBetween(1900, DateTime.UtcNow.Year + 1)
                .WithMessage($"Year must be between 1900 and {DateTime.UtcNow.Year + 1}");
        });

        RuleFor(x => x.ReservePrice)
            .GreaterThanOrEqualTo(0).WithMessage("Reserve price must be non-negative");

        RuleFor(x => x.AuctionEnd)
            .GreaterThan(DateTimeOffset.UtcNow.AddHours(1))
            .WithMessage("Auction end date must be at least 1 hour in the future");

        RuleFor(x => x.SellerUsername)
            .NotEmpty().WithMessage("Seller is required");

        RuleFor(x => x.SellerId)
            .NotEmpty().WithMessage("Seller ID is required");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .MaximumLength(3).WithMessage("Currency must be a valid 3-letter code");
    }
}

