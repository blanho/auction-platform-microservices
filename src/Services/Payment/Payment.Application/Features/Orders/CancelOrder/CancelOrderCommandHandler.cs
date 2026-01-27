using AutoMapper;
using Payment.Application.DTOs;
using Payment.Application.Errors;
using Payment.Application.Interfaces;
using Payment.Domain.Entities;

namespace Payment.Application.Features.Orders.CancelOrder;

public class CancelOrderCommandHandler : ICommandHandler<CancelOrderCommand, OrderDto>
{
    private readonly IOrderRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<CancelOrderCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public CancelOrderCommandHandler(
        IOrderRepository repository,
        IMapper mapper,
        ILogger<CancelOrderCommandHandler> logger,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<OrderDto>> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _repository.GetByIdAsync(request.OrderId);
        if (order == null)
        {
            return Result.Failure<OrderDto>(PaymentErrors.Order.NotFoundById(request.OrderId));
        }

        if (order.Status == OrderStatus.Cancelled)
        {
            return Result.Failure<OrderDto>(PaymentErrors.Order.CancelledById(request.OrderId));
        }

        if (order.Status == OrderStatus.Shipped || order.Status == OrderStatus.Delivered)
        {
            return Result.Failure<OrderDto>(PaymentErrors.Order.AlreadyShippedById(request.OrderId));
        }

        _logger.LogInformation("Cancelling order {OrderId}. Reason: {Reason}", 
            request.OrderId, request.Reason ?? "Not specified");

        order.Cancel(request.Reason);

        var updated = await _repository.UpdateAsync(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Order {OrderId} cancelled", updated.Id);

        return _mapper.Map<OrderDto>(updated);
    }
}
