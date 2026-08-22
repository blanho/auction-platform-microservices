using AutoMapper;
using Payment.Application.DTOs;
using Payment.Application.Errors;
using Payment.Application.Interfaces;

namespace Payment.Application.Features.Orders.GetOrderById;

public class GetOrderByIdQueryHandler : IQueryHandler<GetOrderByIdQuery, OrderDto?>
{
    private readonly IOrderRepository _repository;
    private readonly IMapper _mapper;

    public GetOrderByIdQueryHandler(IOrderRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<OrderDto?>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _repository.GetByIdAsync(request.OrderId);
        if (order is null ||
            (!request.CanViewAll && order.BuyerId != request.UserId && order.SellerId != request.UserId))
        {
            return Result.Failure<OrderDto?>(PaymentErrors.Order.NotFoundById(request.OrderId));
        }

        return order.ToDto(_mapper);
    }
}
