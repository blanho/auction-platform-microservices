using BuildingBlocks.Domain.Constants;
using FluentValidation;

namespace Auctions.Application.Commands.UpdateAuction;

public class UpdateAuctionCommandValidator : AbstractValidator<UpdateAuctionCommand>
{
    public UpdateAuctionCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("Auction ID"));

        When(x => x.Title != null, () =>
        {
            RuleFor(x => x.Title)
                .MaximumLength(ValidationConstants.StringLength.Medium)
                .WithMessage(ValidationConstants.Messages.MaxLength("Title", ValidationConstants.StringLength.Medium))
                .MinimumLength(ValidationConstants.MinLength.Name)
                .WithMessage(ValidationConstants.Messages.MinLength("Title", ValidationConstants.MinLength.Name));
        });

        When(x => x.Description != null, () =>
        {
            RuleFor(x => x.Description)
                .MaximumLength(ValidationConstants.StringLength.Large)
                .WithMessage(ValidationConstants.Messages.MaxLength("Description", ValidationConstants.StringLength.Large))
                .MinimumLength(10).WithMessage(ValidationConstants.Messages.MinLength("Description", 10));
        });

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
    }
}

