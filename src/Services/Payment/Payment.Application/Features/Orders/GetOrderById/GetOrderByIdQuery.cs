using Payment.Application.DTOs;

namespace Payment.Application.Features.Orders.GetOrderById;

public record GetOrderByIdQuery(Guid OrderId) : IQuery<OrderDto?>;
