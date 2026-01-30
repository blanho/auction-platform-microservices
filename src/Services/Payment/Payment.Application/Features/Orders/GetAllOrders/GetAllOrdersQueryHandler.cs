using AutoMapper;
using BuildingBlocks.Application.Abstractions;
using Payment.Application.DTOs;
using Payment.Application.Interfaces;

namespace Payment.Application.Features.Orders.GetAllOrders;

public class GetAllOrdersQueryHandler : IQueryHandler<GetAllOrdersQuery, PaginatedResult<OrderDto>>
{
    private readonly IOrderRepository _repository;
    private readonly IMapper _mapper;

    public GetAllOrdersQueryHandler(IOrderRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<PaginatedResult<OrderDto>>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
    {
        var queryParams = new OrderQueryParams
        {
            Page = request.Page,
            PageSize = request.PageSize,
            Filter = new OrderFilter
            {
                SearchTerm = request.SearchTerm,
                Status = request.Status,
                FromDate = request.FromDate,
                ToDate = request.ToDate
            }
        };

        var orders = await _repository.GetAllAsync(queryParams, cancellationToken);

        var result = new PaginatedResult<OrderDto>(
            orders.Items.ToDtoList(_mapper),
            orders.TotalCount,
            orders.Page,
            orders.PageSize
        );

        return result;
    }
}
