using AutoMapper;
using Common.Core.Helpers;
using Common.CQRS.Abstractions;
using PaymentService.Application.DTOs;
using PaymentService.Application.Interfaces;
using PaymentService.Application.Queries.GetOrdersByBuyer;

namespace PaymentService.Application.Queries.GetOrdersBySeller;

public class GetOrdersBySellerQueryHandler : IQueryHandler<GetOrdersBySellerQuery, PagedOrderResult>
{
    private readonly IOrderRepository _repository;
    private readonly IMapper _mapper;

    public GetOrdersBySellerQueryHandler(IOrderRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<PagedOrderResult>> Handle(GetOrdersBySellerQuery request, CancellationToken cancellationToken)
    {
        var orders = await _repository.GetBySellerUsernameAsync(request.SellerUsername, request.Page, request.PageSize);
        var totalCount = await _repository.GetCountBySellerUsernameAsync(request.SellerUsername);

        var result = new PagedOrderResult(
            _mapper.Map<IEnumerable<OrderDto>>(orders),
            totalCount,
            request.Page,
            request.PageSize
        );

        return result;
    }
}
