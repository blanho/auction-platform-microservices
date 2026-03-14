using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Constants;
using Payment.Application.DTOs;
using Payment.Domain.Enums;

namespace Payment.Application.Features.Orders.GetOrdersByBuyer;

public record GetOrdersByBuyerQuery(
    string BuyerUsername,
    string? SearchTerm = null,
    OrderStatus? Status = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    int Page = PaginationDefaults.DefaultPage,
    int PageSize = PaginationDefaults.DefaultPageSize,
    string? SortBy = null,
    bool SortDescending = true
) : IQuery<PaginatedResult<OrderDto>>;
