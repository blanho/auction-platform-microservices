using Payment.Application.DTOs;

namespace Payment.Application.Features.Orders.PrepareCheckout;

public record PrepareCheckoutCommand(
    Guid OrderId,
    Guid BuyerId,
    string ShippingAddress,
    string? BuyerNotes) : ICommand<OrderDto>;
