using AutoMapper;
using Common.Core.Helpers;
using Common.CQRS.Abstractions;
using PaymentService.Application.DTOs;
using PaymentService.Application.Interfaces;

namespace PaymentService.Application.Queries.GetOrderByAuctionId;

public class GetOrderByAuctionIdQueryHandler : IQueryHandler<GetOrderByAuctionIdQuery, OrderDto?>
{
    private readonly IOrderRepository _repository;
    private readonly IMapper _mapper;

    public GetOrderByAuctionIdQueryHandler(IOrderRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<OrderDto?>> Handle(GetOrderByAuctionIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _repository.GetByAuctionIdAsync(request.AuctionId);
        return order == null ? null : _mapper.Map<OrderDto>(order);
    }
}
