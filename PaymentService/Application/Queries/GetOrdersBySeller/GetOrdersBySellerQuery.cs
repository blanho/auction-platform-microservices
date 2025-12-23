using Common.CQRS.Abstractions;
using PaymentService.Application.Queries.GetOrdersByBuyer;

namespace PaymentService.Application.Queries.GetOrdersBySeller;

public record GetOrdersBySellerQuery(
    string SellerUsername,
    int Page = 1,
    int PageSize = 10
) : IQuery<PagedOrderResult>;
