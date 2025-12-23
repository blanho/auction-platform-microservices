using FluentValidation;

namespace PaymentService.Application.Commands.ReleaseFunds;

public class ReleaseFundsCommandValidator : AbstractValidator<ReleaseFundsCommand>
{
    public ReleaseFundsCommandValidator()
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
    }
}
