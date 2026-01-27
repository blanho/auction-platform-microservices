using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Constants;
using Payment.Application.DTOs;

namespace Payment.Application.Features.Orders.GetOrdersBySeller;

public record GetOrdersBySellerQuery(
    string SellerUsername,
    int Page = PaginationDefaults.DefaultPage,
    int PageSize = PaginationDefaults.DefaultPageSize
) : IQuery<PaginatedResult<OrderDto>>;
