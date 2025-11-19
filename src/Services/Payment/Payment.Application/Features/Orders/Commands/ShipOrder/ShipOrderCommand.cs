using Payment.Application.DTOs;

namespace Payment.Application.Features.Orders.Commands.ShipOrder;

public record ShipOrderCommand(
    Guid OrderId,
    string TrackingNumber,
    string ShippingCarrier,
    string? SellerNotes
) : ICommand<OrderDto>;
