using FluentValidation;

namespace AuctionService.Application.Commands.UpdateAuction;

public class UpdateAuctionCommandValidator : AbstractValidator<UpdateAuctionCommand>
{
    public UpdateAuctionCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Auction ID is required");

        When(x => x.Title != null, () =>
        {
            RuleFor(x => x.Title)
                .MaximumLength(200).WithMessage("Title must not exceed 200 characters")
                .MinimumLength(3).WithMessage("Title must be at least 3 characters");
        });

        When(x => x.Description != null, () =>
        {
            RuleFor(x => x.Description)
                .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters")
                .MinimumLength(10).WithMessage("Description must be at least 10 characters");
        });

        When(x => x.Condition != null, () =>
        {
            RuleFor(x => x.Condition)
                .MaximumLength(50).WithMessage("Condition must not exceed 50 characters");
        });

        When(x => x.YearManufactured != null, () =>
        {
            RuleFor(x => x.YearManufactured)
                .InclusiveBetween(1900, DateTime.UtcNow.Year + 1)
                .WithMessage($"Year must be between 1900 and {DateTime.UtcNow.Year + 1}");
        });
    }
}
