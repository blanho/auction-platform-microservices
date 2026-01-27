using BuildingBlocks.Domain.Constants;
using FluentValidation;

namespace Auctions.Application.Features.Categories.UpdateCategory;

public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("Category ID"));

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("Name"))
            .MaximumLength(ValidationConstants.StringLength.Standard)
            .WithMessage(ValidationConstants.Messages.MaxLength("Name", ValidationConstants.StringLength.Standard));

        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("Slug"))
            .MaximumLength(ValidationConstants.StringLength.Standard)
            .WithMessage(ValidationConstants.Messages.MaxLength("Slug", ValidationConstants.StringLength.Standard))
            .Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$").WithMessage(ValidationConstants.Messages.InvalidFormat("Slug"));

        RuleFor(x => x.Icon)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("Icon"))
            .MaximumLength(ValidationConstants.StringLength.Short)
            .WithMessage(ValidationConstants.Messages.MaxLength("Icon", ValidationConstants.StringLength.Short));

        RuleFor(x => x.Description)
            .MaximumLength(ValidationConstants.StringLength.Long)
            .WithMessage(ValidationConstants.Messages.MaxLength("Description", ValidationConstants.StringLength.Long));

        RuleFor(x => x.ImageUrl)
            .MaximumLength(ValidationConstants.StringLength.Long)
            .WithMessage(ValidationConstants.Messages.MaxLength("Image URL", ValidationConstants.StringLength.Long));

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Display order must be non-negative");
    }
}

