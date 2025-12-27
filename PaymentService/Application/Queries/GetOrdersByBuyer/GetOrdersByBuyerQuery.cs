using Common.Core.Constants;
using Common.CQRS.Abstractions;
using PaymentService.Application.DTOs;

namespace PaymentService.Application.Queries.GetOrdersByBuyer;

public record GetOrdersByBuyerQuery(
    string BuyerUsername,
    int Page = PaginationDefaults.DefaultPage,
    int PageSize = PaginationDefaults.DefaultPageSize
) : IQuery<PagedOrderResult>;

public record PagedOrderResult(
    IEnumerable<OrderDto> Items,
    int TotalCount,
    int Page,
    int PageSize
);
