using FluentValidation;

namespace PaymentService.Application.Commands.Deposit;

public class DepositCommandValidator : AbstractValidator<DepositCommand>
{
    public DepositCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("Username is required")
            .MaximumLength(256)
            .WithMessage("Username must not exceed 256 characters");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than zero");

        RuleFor(x => x.PaymentMethod)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.PaymentMethod))
            .WithMessage("Payment method must not exceed 50 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage("Description must not exceed 500 characters");
    }
}
