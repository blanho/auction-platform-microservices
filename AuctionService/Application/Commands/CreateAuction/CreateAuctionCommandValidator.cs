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

        RuleFor(x => x.Make)
            .NotEmpty().WithMessage("Make is required")
            .MaximumLength(100).WithMessage("Make must not exceed 100 characters");

        RuleFor(x => x.Model)
            .NotEmpty().WithMessage("Model is required")
            .MaximumLength(100).WithMessage("Model must not exceed 100 characters");

        RuleFor(x => x.Year)
            .InclusiveBetween(1900, DateTime.UtcNow.Year + 1)
            .WithMessage($"Year must be between 1900 and {DateTime.UtcNow.Year + 1}");

        RuleFor(x => x.Color)
            .NotEmpty().WithMessage("Color is required")
            .MaximumLength(50).WithMessage("Color must not exceed 50 characters");

        RuleFor(x => x.Mileage)
            .GreaterThanOrEqualTo(0).WithMessage("Mileage must be non-negative");

        RuleFor(x => x.ReservePrice)
            .GreaterThanOrEqualTo(0).WithMessage("Reserve price must be non-negative");

        RuleFor(x => x.AuctionEnd)
            .GreaterThan(DateTimeOffset.UtcNow.AddHours(1))
            .WithMessage("Auction end date must be at least 1 hour in the future");

        RuleFor(x => x.Seller)
            .NotEmpty().WithMessage("Seller is required");
    }
}
