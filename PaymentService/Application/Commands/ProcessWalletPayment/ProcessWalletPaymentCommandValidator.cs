using FluentValidation;

namespace PaymentService.Application.Commands.ProcessWalletPayment;

public class ProcessWalletPaymentCommandValidator : AbstractValidator<ProcessWalletPaymentCommand>
{
    public ProcessWalletPaymentCommandValidator()
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

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required")
            .MaximumLength(500)
            .WithMessage("Description must not exceed 500 characters");
    }
}
