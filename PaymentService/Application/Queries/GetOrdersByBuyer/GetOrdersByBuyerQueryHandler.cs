using AutoMapper;
using Common.Core.Helpers;
using Common.CQRS.Abstractions;
using PaymentService.Application.DTOs;
using PaymentService.Application.Interfaces;

namespace PaymentService.Application.Queries.GetOrdersByBuyer;

public class GetOrdersByBuyerQueryHandler : IQueryHandler<GetOrdersByBuyerQuery, PagedOrderResult>
{
    private readonly IOrderRepository _repository;
    private readonly IMapper _mapper;

    public GetOrdersByBuyerQueryHandler(IOrderRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<PagedOrderResult>> Handle(GetOrdersByBuyerQuery request, CancellationToken cancellationToken)
    {
        var orders = await _repository.GetByBuyerUsernameAsync(request.BuyerUsername, request.Page, request.PageSize);
        var totalCount = await _repository.GetCountByBuyerUsernameAsync(request.BuyerUsername);

        var result = new PagedOrderResult(
            _mapper.Map<IEnumerable<OrderDto>>(orders),
            totalCount,
            request.Page,
            request.PageSize
        );

        return result;
    }
}
