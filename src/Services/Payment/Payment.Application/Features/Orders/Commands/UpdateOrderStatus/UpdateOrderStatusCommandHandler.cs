using AutoMapper;
using Payment.Application.DTOs;
using Payment.Application.Interfaces;
using Payment.Domain.Entities;

namespace Payment.Application.Features.Orders.Commands.UpdateOrderStatus;

public class UpdateOrderStatusCommandHandler : ICommandHandler<UpdateOrderStatusCommand, OrderDto>
{
    private readonly IOrderRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateOrderStatusCommandHandler> _logger;
    private readonly BuildingBlocks.Application.Abstractions.Persistence.IUnitOfWork _unitOfWork;

    public UpdateOrderStatusCommandHandler(
        IOrderRepository repository,
        IMapper mapper,
        ILogger<UpdateOrderStatusCommandHandler> logger,
        BuildingBlocks.Application.Abstractions.Persistence.IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<OrderDto>> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await _repository.GetByIdAsync(request.OrderId);
        if (order == null)
        {
            return Result.Failure<OrderDto>(Error.Create("Order.NotFound", $"Order {request.OrderId} not found"));
        }

        if (request.PaymentStatus == PaymentStatus.Completed && order.PaymentStatus != PaymentStatus.Completed)
        {
            order.CompletePayment(request.PaymentTransactionId);
        }

        if (request.Status == OrderStatus.Shipped && order.Status != OrderStatus.Shipped)
        {
            order.MarkAsShipped(request.TrackingNumber, request.ShippingCarrier);
        }
        else if (request.Status == OrderStatus.Delivered && order.Status != OrderStatus.Delivered)
        {
            order.MarkAsDelivered();
        }
        else if (request.Status.HasValue && request.Status.Value != order.Status)
        {
            order.ChangeStatus(request.Status.Value);
        }

        if (!string.IsNullOrEmpty(request.ShippingAddress))
            order.SetShippingAddress(request.ShippingAddress);

        if (!string.IsNullOrEmpty(request.BuyerNotes))
            order.AddBuyerNotes(request.BuyerNotes);

        if (!string.IsNullOrEmpty(request.SellerNotes))
            order.AddSellerNotes(request.SellerNotes);

        var updated = await _repository.UpdateAsync(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated order {OrderId}", updated.Id);

        return _mapper.Map<OrderDto>(updated);
    }
}
