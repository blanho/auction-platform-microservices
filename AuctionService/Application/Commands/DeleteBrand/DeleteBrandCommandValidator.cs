using FluentValidation;

namespace AuctionService.Application.Commands.DeleteBrand;

public class DeleteBrandCommandValidator : AbstractValidator<DeleteBrandCommand>
{
    public DeleteBrandCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Brand ID is required");
    }
}
