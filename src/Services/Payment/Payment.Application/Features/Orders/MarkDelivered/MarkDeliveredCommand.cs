using Payment.Application.DTOs;

namespace Payment.Application.Features.Orders.MarkDelivered;

public record MarkDeliveredCommand(
    Guid OrderId,
    Guid BuyerId,
    bool CanManageAll) : ICommand<OrderDto>;
