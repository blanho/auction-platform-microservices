using Payment.Application.DTOs;

namespace Payment.Application.Features.Orders.Commands.ProcessPayment;

public record ProcessPaymentCommand(
    Guid OrderId,
    string PaymentMethod,
    string? ExternalTransactionId
) : ICommand<OrderDto>;
