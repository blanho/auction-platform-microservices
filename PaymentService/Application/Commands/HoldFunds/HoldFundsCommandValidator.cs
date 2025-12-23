using FluentValidation;

namespace PaymentService.Application.Commands.HoldFunds;

public class HoldFundsCommandValidator : AbstractValidator<HoldFundsCommand>
{
    public HoldFundsCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("Username is required")
            .MaximumLength(256)
            .WithMessage("Username must not exceed 256 characters");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than zero");

        RuleFor(x => x.ReferenceId)
            .NotEmpty()
            .WithMessage("Reference ID is required");

        RuleFor(x => x.ReferenceType)
            .NotEmpty()
            .WithMessage("Reference type is required")
            .MaximumLength(50)
            .WithMessage("Reference type must not exceed 50 characters");
    }
}
