using FluentValidation;

namespace AuctionService.Application.Commands.UpdateAuction;

/// <summary>
/// FluentValidation validator for UpdateAuctionCommand
/// </summary>
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

        When(x => x.Make != null, () =>
        {
            RuleFor(x => x.Make)
                .MaximumLength(100).WithMessage("Make must not exceed 100 characters");
        });

        When(x => x.Model != null, () =>
        {
            RuleFor(x => x.Model)
                .MaximumLength(100).WithMessage("Model must not exceed 100 characters");
        });

        When(x => x.Year != null, () =>
        {
            RuleFor(x => x.Year)
                .InclusiveBetween(1900, DateTime.UtcNow.Year + 1)
                .WithMessage($"Year must be between 1900 and {DateTime.UtcNow.Year + 1}");
        });

        When(x => x.Color != null, () =>
        {
            RuleFor(x => x.Color)
                .MaximumLength(50).WithMessage("Color must not exceed 50 characters");
        });

        When(x => x.Mileage != null, () =>
        {
            RuleFor(x => x.Mileage)
                .GreaterThanOrEqualTo(0).WithMessage("Mileage must be non-negative");
        });
    }
}
