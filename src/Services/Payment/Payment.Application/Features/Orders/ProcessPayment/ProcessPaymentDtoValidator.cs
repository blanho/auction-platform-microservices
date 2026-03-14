using BuildingBlocks.Domain.Constants;
using FluentValidation;
using Payment.Application.DTOs;
using Payment.Domain.Constants;

namespace Payment.Application.Features.Orders.ProcessPayment;

public class ProcessPaymentDtoValidator : AbstractValidator<ProcessPaymentDto>
{
    public ProcessPaymentDtoValidator()
    {
        RuleFor(x => x.PaymentMethod)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("Payment method"))
            .Must(PaymentMethods.IsValid).WithMessage(ValidationConstants.Messages.Invalid("Payment method"));

        When(x => !string.IsNullOrEmpty(x.ExternalTransactionId), () =>
        {
            RuleFor(x => x.ExternalTransactionId)
                .MaximumLength(ValidationConstants.StringLength.Username)
                .WithMessage(ValidationConstants.Messages.MaxLength("External transaction ID", ValidationConstants.StringLength.Username));
        });
    }
}
