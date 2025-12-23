using Common.CQRS.Abstractions;
using PaymentService.Application.DTOs;
using PaymentService.Domain.Entities;

namespace PaymentService.Application.Commands.UpdateOrderStatus;

public record UpdateOrderStatusCommand(
    Guid OrderId,
    OrderStatus? Status,
    PaymentStatus? PaymentStatus,
    string? PaymentTransactionId,
    string? ShippingAddress,
    string? TrackingNumber,
    string? ShippingCarrier,
    string? BuyerNotes,
    string? SellerNotes
) : ICommand<OrderDto>;
