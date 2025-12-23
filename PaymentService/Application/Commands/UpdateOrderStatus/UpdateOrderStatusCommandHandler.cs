using AutoMapper;
using Common.Core.Helpers;
using Common.CQRS.Abstractions;
using Common.Repository.Interfaces;
using PaymentService.Application.DTOs;
using PaymentService.Application.Interfaces;
using PaymentService.Domain.Entities;

namespace PaymentService.Application.Commands.UpdateOrderStatus;

public class UpdateOrderStatusCommandHandler : ICommandHandler<UpdateOrderStatusCommand, OrderDto>
{
    private readonly IOrderRepository _repository;
    private readonly IMapper _mapper;
    private readonly IAppLogger<UpdateOrderStatusCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateOrderStatusCommandHandler(
        IOrderRepository repository,
        IMapper mapper,
        IAppLogger<UpdateOrderStatusCommandHandler> logger,
        IUnitOfWork unitOfWork)
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
        else if (request.PaymentStatus.HasValue && request.PaymentStatus != order.PaymentStatus)
        {
            order.PaymentStatus = request.PaymentStatus.Value;
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

        if (!string.IsNullOrEmpty(request.PaymentTransactionId) && order.PaymentTransactionId != request.PaymentTransactionId)
            order.PaymentTransactionId = request.PaymentTransactionId;

        if (!string.IsNullOrEmpty(request.ShippingAddress))
            order.ShippingAddress = request.ShippingAddress;

        if (!string.IsNullOrEmpty(request.TrackingNumber) && order.TrackingNumber != request.TrackingNumber)
            order.TrackingNumber = request.TrackingNumber;

        if (!string.IsNullOrEmpty(request.ShippingCarrier) && order.ShippingCarrier != request.ShippingCarrier)
            order.ShippingCarrier = request.ShippingCarrier;

        if (!string.IsNullOrEmpty(request.BuyerNotes))
            order.BuyerNotes = request.BuyerNotes;

        if (!string.IsNullOrEmpty(request.SellerNotes))
            order.SellerNotes = request.SellerNotes;

        var updated = await _repository.UpdateAsync(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated order {OrderId}", updated.Id);

        return _mapper.Map<OrderDto>(updated);
    }
}
