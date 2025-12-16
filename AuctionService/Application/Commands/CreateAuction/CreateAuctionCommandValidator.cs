using FluentValidation;

namespace AuctionService.Application.Commands.CreateAuction;

public class CreateAuctionCommandValidator : AbstractValidator<CreateAuctionCommand>
{
    public CreateAuctionCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters")
            .MinimumLength(3).WithMessage("Title must be at least 3 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters")
            .MinimumLength(10).WithMessage("Description must be at least 10 characters");

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
