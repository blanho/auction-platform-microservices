using BuildingBlocks.Domain.Constants;
using FluentValidation;

namespace Auctions.Application.Commands.AddSellerResponse;

public class AddSellerResponseCommandValidator : AbstractValidator<AddSellerResponseCommand>
{
    public AddSellerResponseCommandValidator()
    {
        RuleFor(x => x.ReviewId)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("Review ID"));

        RuleFor(x => x.SellerUsername)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("Seller username"))
            .MaximumLength(ValidationConstants.StringLength.Username)
            .WithMessage(ValidationConstants.Messages.MaxLength("Seller username", ValidationConstants.StringLength.Username));

        RuleFor(x => x.Response)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("Response"))
            .MinimumLength(10).WithMessage(ValidationConstants.Messages.MinLength("Response", 10))
            .MaximumLength(ValidationConstants.StringLength.Large)
            .WithMessage(ValidationConstants.Messages.MaxLength("Response", ValidationConstants.StringLength.Large));
    }
}

