using Payment.Application.DTOs;
using Payment.Domain.Entities;

namespace Payment.Application.Features.Orders.Commands.UpdateOrderStatus;

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
