using Payment.Application.DTOs;

namespace Payment.Application.Features.Orders.Queries.GetOrderById;

public record GetOrderByIdQuery(Guid OrderId) : IQuery<OrderDto?>;
