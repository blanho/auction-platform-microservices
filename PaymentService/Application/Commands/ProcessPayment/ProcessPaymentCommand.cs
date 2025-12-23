using Common.CQRS.Abstractions;
using PaymentService.Application.DTOs;

namespace PaymentService.Application.Commands.ProcessPayment;

public record ProcessPaymentCommand(
    Guid OrderId,
    string PaymentMethod,
    string? ExternalTransactionId
) : ICommand<OrderDto>;
