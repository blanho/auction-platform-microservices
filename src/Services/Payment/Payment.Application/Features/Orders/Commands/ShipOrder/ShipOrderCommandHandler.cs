using AutoMapper;
using Payment.Application.DTOs;
using Payment.Application.Interfaces;
using Payment.Domain.Entities;

namespace Payment.Application.Features.Orders.Commands.ShipOrder;

public class ShipOrderCommandHandler : ICommandHandler<ShipOrderCommand, OrderDto>
{
    private readonly IOrderRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<ShipOrderCommandHandler> _logger;
    private readonly BuildingBlocks.Application.Abstractions.Persistence.IUnitOfWork _unitOfWork;

    public ShipOrderCommandHandler(
        IOrderRepository repository,
        IMapper mapper,
        ILogger<ShipOrderCommandHandler> logger,
        BuildingBlocks.Application.Abstractions.Persistence.IUnitOfWork unitOfWork)
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
            return Result.Failure<OrderDto>(Error.Create("Order.NotFound", $"Order {request.OrderId} not found"));
        }

        if (order.PaymentStatus != PaymentStatus.Completed)
        {
            return Result.Failure<OrderDto>(Error.Create("Order.NotPaid", $"Order {request.OrderId} has not been paid yet"));
        }

        if (order.Status == OrderStatus.Shipped || order.Status == OrderStatus.Delivered)
        {
            return Result.Failure<OrderDto>(Error.Create("Order.AlreadyShipped", $"Order {request.OrderId} has already been shipped"));
        }

        if (order.Status == OrderStatus.Cancelled)
        {
            return Result.Failure<OrderDto>(Error.Create("Order.Cancelled", $"Order {request.OrderId} has been cancelled"));
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
