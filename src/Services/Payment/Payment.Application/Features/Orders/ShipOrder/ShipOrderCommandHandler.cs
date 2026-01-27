using AutoMapper;
using Payment.Application.DTOs;
using Payment.Application.Errors;
using Payment.Application.Interfaces;
using Payment.Domain.Entities;

namespace Payment.Application.Features.Orders.ShipOrder;

public class ShipOrderCommandHandler : ICommandHandler<ShipOrderCommand, OrderDto>
{
    private readonly IOrderRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<ShipOrderCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public ShipOrderCommandHandler(
        IOrderRepository repository,
        IMapper mapper,
        ILogger<ShipOrderCommandHandler> logger,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<OrderDto>> Handle(ShipOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _repository.GetByIdAsync(request.OrderId);
        if (order == null)
        {
            return Result.Failure<OrderDto>(PaymentErrors.Order.NotFoundById(request.OrderId));
        }

        if (order.PaymentStatus != PaymentStatus.Completed)
        {
            return Result.Failure<OrderDto>(PaymentErrors.Order.NotPaidById(request.OrderId));
        }

        if (order.Status == OrderStatus.Shipped || order.Status == OrderStatus.Delivered)
        {
            return Result.Failure<OrderDto>(PaymentErrors.Order.AlreadyShippedById(request.OrderId));
        }

        if (order.Status == OrderStatus.Cancelled)
        {
            return Result.Failure<OrderDto>(PaymentErrors.Order.CancelledById(request.OrderId));
        }

        _logger.LogInformation("Shipping order {OrderId} via {Carrier} with tracking {TrackingNumber}", 
            request.OrderId, request.ShippingCarrier, request.TrackingNumber);

        order.MarkAsShipped(request.TrackingNumber, request.ShippingCarrier);

        if (!string.IsNullOrEmpty(request.SellerNotes))
        {
            order.AddSellerNotes(request.SellerNotes);
        }

        var updated = await _repository.UpdateAsync(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Order {OrderId} marked as shipped", updated.Id);

        return updated.ToDto(_mapper);
    }
}
