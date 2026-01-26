using Payment.Application.DTOs;

namespace Payment.Application.Features.Orders.Commands.CancelOrder;

public record CancelOrderCommand(Guid OrderId, string? Reason) : ICommand<OrderDto>;
