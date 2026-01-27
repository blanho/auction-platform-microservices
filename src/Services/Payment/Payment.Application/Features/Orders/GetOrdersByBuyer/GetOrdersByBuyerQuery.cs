using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Constants;
using Payment.Application.DTOs;

namespace Payment.Application.Features.Orders.GetOrdersByBuyer;

public record GetOrdersByBuyerQuery(
    string BuyerUsername,
    int Page = PaginationDefaults.DefaultPage,
    int PageSize = PaginationDefaults.DefaultPageSize
) : IQuery<PaginatedResult<OrderDto>>;
