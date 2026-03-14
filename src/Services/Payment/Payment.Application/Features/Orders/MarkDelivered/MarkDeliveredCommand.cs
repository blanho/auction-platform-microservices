using Payment.Application.DTOs;

namespace Payment.Application.Features.Orders.MarkDelivered;

public record MarkDeliveredCommand(Guid OrderId) : ICommand<OrderDto>;
