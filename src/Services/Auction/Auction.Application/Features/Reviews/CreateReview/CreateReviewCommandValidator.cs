using BuildingBlocks.Domain.Constants;
using FluentValidation;

namespace Auctions.Application.Features.Reviews.CreateReview;

public class CreateReviewCommandValidator : AbstractValidator<CreateReviewCommand>
{
    public CreateReviewCommandValidator()
    {
        RuleFor(x => x.AuctionId)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("Auction ID"));

        RuleFor(x => x.ReviewerId)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("Reviewer ID"));

        RuleFor(x => x.ReviewerUsername)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("Reviewer username"))
            .MaximumLength(ValidationConstants.StringLength.Username)
            .WithMessage(ValidationConstants.Messages.MaxLength("Reviewer username", ValidationConstants.StringLength.Username));

        RuleFor(x => x.ReviewedUserId)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("Reviewed user ID"));

        RuleFor(x => x.ReviewedUsername)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("Reviewed username"))
            .MaximumLength(ValidationConstants.StringLength.Username)
            .WithMessage(ValidationConstants.Messages.MaxLength("Reviewed username", ValidationConstants.StringLength.Username));

        RuleFor(x => x.Rating)
            .InclusiveBetween(ValidationConstants.NumericRange.MinRating, ValidationConstants.NumericRange.MaxRating)
            .WithMessage(ValidationConstants.Messages.OutOfRange("Rating", ValidationConstants.NumericRange.MinRating, ValidationConstants.NumericRange.MaxRating));

        When(x => x.Title != null, () =>
        {
            RuleFor(x => x.Title)
                .MaximumLength(ValidationConstants.StringLength.Medium)
                .WithMessage(ValidationConstants.Messages.MaxLength("Title", ValidationConstants.StringLength.Medium));
        });

        When(x => x.Comment != null, () =>
        {
            RuleFor(x => x.Comment)
                .MaximumLength(ValidationConstants.StringLength.Large)
                .WithMessage(ValidationConstants.Messages.MaxLength("Comment", ValidationConstants.StringLength.Large));
        });
    }
}

