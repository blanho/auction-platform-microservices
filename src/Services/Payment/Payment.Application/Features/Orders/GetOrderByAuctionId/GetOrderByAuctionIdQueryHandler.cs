using AutoMapper;
using Payment.Application.DTOs;
using Payment.Application.Errors;
using Payment.Application.Interfaces;

namespace Payment.Application.Features.Orders.GetOrderByAuctionId;

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
        if (order is null ||
            (!request.CanViewAll && order.BuyerId != request.UserId && order.SellerId != request.UserId))
        {
            return Result.Failure<OrderDto?>(PaymentErrors.Order.NotFound);
        }

        return order.ToDto(_mapper);
    }
}
