using FluentValidation;

namespace AuctionService.Application.Commands.CreateReview;

public class CreateReviewCommandValidator : AbstractValidator<CreateReviewCommand>
{
    public CreateReviewCommandValidator()
    {
        RuleFor(x => x.AuctionId)
            .NotEmpty().WithMessage("Auction ID is required");

        RuleFor(x => x.ReviewerId)
            .NotEmpty().WithMessage("Reviewer ID is required");

        RuleFor(x => x.ReviewerUsername)
            .NotEmpty().WithMessage("Reviewer username is required")
            .MaximumLength(256).WithMessage("Reviewer username must not exceed 256 characters");

        RuleFor(x => x.ReviewedUserId)
            .NotEmpty().WithMessage("Reviewed user ID is required");

        RuleFor(x => x.ReviewedUsername)
            .NotEmpty().WithMessage("Reviewed username is required")
            .MaximumLength(256).WithMessage("Reviewed username must not exceed 256 characters");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5");

        When(x => x.Title != null, () =>
        {
            RuleFor(x => x.Title)
                .MaximumLength(200).WithMessage("Title must not exceed 200 characters");
        });

        When(x => x.Comment != null, () =>
        {
            RuleFor(x => x.Comment)
                .MaximumLength(2000).WithMessage("Comment must not exceed 2000 characters");
        });
    }
}
