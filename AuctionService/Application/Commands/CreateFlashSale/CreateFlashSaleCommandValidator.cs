using FluentValidation;

namespace AuctionService.Application.Commands.CreateFlashSale;

public class CreateFlashSaleCommandValidator : AbstractValidator<CreateFlashSaleCommand>
{
    public CreateFlashSaleCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters");

        RuleFor(x => x.BannerUrl)
            .MaximumLength(500).WithMessage("Banner URL must not exceed 500 characters");

        RuleFor(x => x.StartTime)
            .NotEmpty().WithMessage("Start time is required");

        RuleFor(x => x.EndTime)
            .NotEmpty().WithMessage("End time is required")
            .GreaterThan(x => x.StartTime).WithMessage("End time must be after start time");

        RuleFor(x => x.DiscountPercentage)
            .InclusiveBetween(1, 100).WithMessage("Discount percentage must be between 1 and 100");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Display order must be non-negative");
    }
}
