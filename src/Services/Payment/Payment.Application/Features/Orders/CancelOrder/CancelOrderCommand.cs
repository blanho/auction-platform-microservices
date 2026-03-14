using Payment.Application.DTOs;

namespace Payment.Application.Features.Orders.CancelOrder;

public record CancelOrderCommand(Guid OrderId, string? Reason) : ICommand<OrderDto>;
