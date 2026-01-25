using AutoMapper;
using BuildingBlocks.Application.Abstractions;
using Payment.Application.DTOs;
using Payment.Application.Interfaces;

namespace Payment.Application.Features.Orders.Queries.GetOrdersBySeller;

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
        var orders = await _repository.GetBySellerUsernameAsync(request.SellerUsername, request.Page, request.PageSize);
        var totalCount = await _repository.GetCountBySellerUsernameAsync(request.SellerUsername);

        var result = new PaginatedResult<OrderDto>(
            orders.ToDtoList(_mapper),
            totalCount,
            request.Page,
            request.PageSize
        );

        return result;
    }
}
