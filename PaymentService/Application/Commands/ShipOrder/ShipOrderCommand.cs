using Common.CQRS.Abstractions;
using PaymentService.Application.DTOs;

namespace PaymentService.Application.Commands.ShipOrder;

public record ShipOrderCommand(
    Guid OrderId,
    string TrackingNumber,
    string ShippingCarrier,
    string? SellerNotes
) : ICommand<OrderDto>;
