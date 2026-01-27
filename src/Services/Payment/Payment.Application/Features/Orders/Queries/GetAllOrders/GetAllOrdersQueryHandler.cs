using AutoMapper;
using BuildingBlocks.Application.Abstractions;
using Payment.Application.DTOs;
using Payment.Application.Interfaces;

namespace Payment.Application.Features.Orders.Queries.GetAllOrders;

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
        var orders = await _repository.GetAllAsync(
            request.Page, 
            request.PageSize, 
            request.SearchTerm, 
            request.Status,
            request.FromDate,
            request.ToDate);
            
        var totalCount = await _repository.GetAllCountAsync(
            request.SearchTerm, 
            request.Status,
            request.FromDate,
            request.ToDate);

        var result = new PaginatedResult<OrderDto>(
            orders.ToDtoList(_mapper),
            totalCount,
            request.Page,
            request.PageSize
        );

        return result;
    }
}
