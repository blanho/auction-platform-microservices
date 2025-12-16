using FluentValidation;

namespace AuctionService.Application.Commands.UpdateFlashSale;

public class UpdateFlashSaleCommandValidator : AbstractValidator<UpdateFlashSaleCommand>
{
    public UpdateFlashSaleCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Flash sale ID is required");

        RuleFor(x => x.Title)
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Title));

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters")
            .When(x => x.Description != null);

        RuleFor(x => x.BannerUrl)
            .MaximumLength(500).WithMessage("Banner URL must not exceed 500 characters")
            .When(x => x.BannerUrl != null);

        RuleFor(x => x.DiscountPercentage)
            .InclusiveBetween(1, 100).WithMessage("Discount percentage must be between 1 and 100")
            .When(x => x.DiscountPercentage.HasValue);

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Display order must be non-negative")
            .When(x => x.DisplayOrder.HasValue);
    }
}
