using Common.CQRS.Abstractions;
using PaymentService.Application.DTOs;

namespace PaymentService.Application.Queries.GetOrderById;

public record GetOrderByIdQuery(Guid OrderId) : IQuery<OrderDto?>;
