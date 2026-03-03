using BuildingBlocks.Domain.Constants;
using FluentValidation;

namespace Auctions.Application.Features.Auctions.CreateAuction;

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
                .WithMessage(ValidationConstants.Messages.OutOfRange("Year manufactured", 1900, DateTime.UtcNow.Year + 1));
        });

        RuleFor(x => x.ReservePrice)
            .GreaterThanOrEqualTo(0).WithMessage(ValidationConstants.Messages.MustBeNonNegative("Reserve price"));

        RuleFor(x => x.AuctionEnd)
            .GreaterThan(DateTimeOffset.UtcNow.AddHours(1))
            .WithMessage(ValidationConstants.Messages.MustBeAtLeast("Auction end date", 1));

        RuleFor(x => x.SellerUsername)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("Seller username"));

        RuleFor(x => x.SellerId)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("Seller ID"));

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("Currency"))
            .MaximumLength(ValidationConstants.MinLength.ShortCode + 1)
            .WithMessage(ValidationConstants.Messages.MaxLength("Currency", ValidationConstants.MinLength.ShortCode + 1));
    }
}

