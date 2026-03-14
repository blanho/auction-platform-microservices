using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Constants;
using Payment.Application.DTOs;
using Payment.Domain.Enums;

namespace Payment.Application.Features.Orders.GetAllOrders;

public record GetAllOrdersQuery(
    int Page = PaginationDefaults.DefaultPage,
    int PageSize = PaginationDefaults.DefaultPageSize,
    string? SearchTerm = null,
    OrderStatus? Status = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null
) : IQuery<PaginatedResult<OrderDto>>;
