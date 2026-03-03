using BuildingBlocks.Domain.Constants;
using FluentValidation;

namespace Auctions.Application.Features.Brands.DeleteBrand;

public class DeleteBrandCommandValidator : AbstractValidator<DeleteBrandCommand>
{
    public DeleteBrandCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage(ValidationConstants.Messages.Required("Brand ID"));
    }
}

