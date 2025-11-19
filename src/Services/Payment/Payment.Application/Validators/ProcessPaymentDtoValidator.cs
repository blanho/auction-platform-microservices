using BuildingBlocks.Domain.Constants;
using FluentValidation;
using Payment.Application.DTOs;

namespace Payment.Application.Validators;

public class ProcessPaymentDtoValidator : AbstractValidator<ProcessPaymentDto>
{
    private static readonly string[] AllowedPaymentMethods = 
    { 
        "stripe", "paypal", "wallet", "bank_transfer", "credit_card" 
    };

    public ProcessPaymentDtoValidator()
    {
        RuleFor(x => x.PaymentMethod)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("Payment method"))
            .Must(BeAllowedPaymentMethod).WithMessage("Invalid payment method");

        When(x => !string.IsNullOrEmpty(x.ExternalTransactionId), () =>
        {
            RuleFor(x => x.ExternalTransactionId)
                .MaximumLength(256).WithMessage(ValidationConstants.Messages.MaxLength("External transaction ID", 256));
        });
    }

    private static bool BeAllowedPaymentMethod(string paymentMethod)
    {
        return AllowedPaymentMethods.Contains(paymentMethod.ToLowerInvariant());
    }
}
