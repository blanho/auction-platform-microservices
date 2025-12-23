using FluentValidation;
using PaymentService.Application.DTOs;

namespace PaymentService.Application.Validators;

public class ProcessPaymentDtoValidator : AbstractValidator<ProcessPaymentDto>
{
    private static readonly string[] AllowedPaymentMethods = 
    { 
        "stripe", "paypal", "wallet", "bank_transfer", "credit_card" 
    };

    public ProcessPaymentDtoValidator()
    {
        RuleFor(x => x.PaymentMethod)
            .NotEmpty().WithMessage("Payment method is required")
            .Must(BeAllowedPaymentMethod).WithMessage("Invalid payment method");

        When(x => !string.IsNullOrEmpty(x.ExternalTransactionId), () =>
        {
            RuleFor(x => x.ExternalTransactionId)
                .MaximumLength(256).WithMessage("External transaction ID must not exceed 256 characters");
        });
    }

    private static bool BeAllowedPaymentMethod(string paymentMethod)
    {
        return AllowedPaymentMethods.Contains(paymentMethod.ToLowerInvariant());
    }
}
