using Payment.Application.DTOs;

namespace Payment.Application.Features.Orders.Commands.MarkDelivered;

public record MarkDeliveredCommand(Guid OrderId) : ICommand<OrderDto>;
