using AutoMapper;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Paging;
using Payment.Application.DTOs;
using Payment.Application.Interfaces;

namespace Payment.Application.Features.Orders.GetOrdersBySeller;

public class GetOrdersBySellerQueryHandler : IQueryHandler<GetOrdersBySellerQuery, PaginatedResult<OrderDto>>
{
    private readonly IOrderRepository _repository;
    private readonly IMapper _mapper;

    public GetOrdersBySellerQueryHandler(IOrderRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<PaginatedResult<OrderDto>>> Handle(GetOrdersBySellerQuery request, CancellationToken cancellationToken)
    {
        var queryParams = QueryParameters.Create(request.Page, request.PageSize);
        var orders = await _repository.GetBySellerUsernameAsync(request.SellerUsername, queryParams);

        var result = new PaginatedResult<OrderDto>(
            orders.Items.ToDtoList(_mapper),
            orders.TotalCount,
            orders.Page,
            orders.PageSize
        );

        return result;
    }
}
