using AutoMapper;
using BuildingBlocks.Application.Abstractions;
using Payment.Application.DTOs;
using Payment.Application.Filtering;
using Payment.Application.Interfaces;

namespace Payment.Application.Features.Orders.GetOrdersByBuyer;

public class GetOrdersByBuyerQueryHandler : IQueryHandler<GetOrdersByBuyerQuery, PaginatedResult<OrderDto>>
{
    private readonly IOrderRepository _repository;
    private readonly IMapper _mapper;

    public GetOrdersByBuyerQueryHandler(IOrderRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<PaginatedResult<OrderDto>>> Handle(GetOrdersByBuyerQuery request, CancellationToken cancellationToken)
    {
        var queryParams = new OrderQueryParams
        {
            Page = request.Page,
            PageSize = request.PageSize,
            SortBy = request.SortBy,
            SortDescending = request.SortDescending,
            Filter = new OrderFilter
            {
                BuyerUsername = request.BuyerUsername,
                SearchTerm = request.SearchTerm,
                Status = request.Status,
                FromDate = request.FromDate,
                ToDate = request.ToDate
            }
        };
        
        var orders = await _repository.GetByBuyerUsernameAsync(queryParams);

        var result = new PaginatedResult<OrderDto>(
            orders.Items.ToDtoList(_mapper),
            orders.TotalCount,
            orders.Page,
            orders.PageSize
        );

        return result;
    }
}
