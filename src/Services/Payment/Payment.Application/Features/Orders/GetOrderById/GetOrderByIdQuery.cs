using Payment.Application.DTOs;

namespace Payment.Application.Features.Orders.GetOrderById;

public record GetOrderByIdQuery(
    Guid OrderId,
    Guid UserId,
    bool CanViewAll) : IQuery<OrderDto?>;
