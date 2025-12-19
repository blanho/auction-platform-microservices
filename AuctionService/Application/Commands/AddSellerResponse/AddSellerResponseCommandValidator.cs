using FluentValidation;

namespace AuctionService.Application.Commands.AddSellerResponse;

public class AddSellerResponseCommandValidator : AbstractValidator<AddSellerResponseCommand>
{
    public AddSellerResponseCommandValidator()
    {
        RuleFor(x => x.ReviewId)
            .NotEmpty().WithMessage("Review ID is required");

        RuleFor(x => x.SellerUsername)
            .NotEmpty().WithMessage("Seller username is required")
            .MaximumLength(256).WithMessage("Seller username must not exceed 256 characters");

        RuleFor(x => x.Response)
            .NotEmpty().WithMessage("Response is required")
            .MinimumLength(10).WithMessage("Response must be at least 10 characters")
            .MaximumLength(2000).WithMessage("Response must not exceed 2000 characters");
    }
}
