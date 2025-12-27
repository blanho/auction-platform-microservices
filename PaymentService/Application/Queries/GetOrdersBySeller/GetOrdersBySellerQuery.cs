using Common.Core.Constants;
using Common.CQRS.Abstractions;
using PaymentService.Application.Queries.GetOrdersByBuyer;

namespace PaymentService.Application.Queries.GetOrdersBySeller;

public record GetOrdersBySellerQuery(
    string SellerUsername,
    int Page = PaginationDefaults.DefaultPage,
    int PageSize = PaginationDefaults.DefaultPageSize
) : IQuery<PagedOrderResult>;
