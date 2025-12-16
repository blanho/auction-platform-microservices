using FluentValidation;

namespace AuctionService.Application.Commands.AddFlashSaleItem;

public class AddFlashSaleItemCommandValidator : AbstractValidator<AddFlashSaleItemCommand>
{
    public AddFlashSaleItemCommandValidator()
    {
        RuleFor(x => x.FlashSaleId)
            .NotEmpty().WithMessage("Flash sale ID is required");

        RuleFor(x => x.AuctionId)
            .NotEmpty().WithMessage("Auction ID is required");

        RuleFor(x => x.SpecialPrice)
            .GreaterThan(0).WithMessage("Special price must be greater than 0")
            .When(x => x.SpecialPrice.HasValue);

        RuleFor(x => x.DiscountPercentage)
            .InclusiveBetween(1, 100).WithMessage("Discount percentage must be between 1 and 100")
            .When(x => x.DiscountPercentage.HasValue);

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Display order must be non-negative");
    }
}
