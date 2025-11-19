using AutoMapper;
using BuildingBlocks.Application.Abstractions;
using Payment.Application.DTOs;
using Payment.Application.Interfaces;

namespace Payment.Application.Features.Orders.Queries.GetOrdersByBuyer;

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
        var orders = await _repository.GetByBuyerUsernameAsync(request.BuyerUsername, request.Page, request.PageSize);
        var totalCount = await _repository.GetCountByBuyerUsernameAsync(request.BuyerUsername);

        var result = new PaginatedResult<OrderDto>(
            _mapper.Map<List<OrderDto>>(orders),
            totalCount,
            request.Page,
            request.PageSize
        );

        return result;
    }
}
