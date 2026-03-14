using AutoMapper;
using BuildingBlocks.Application.Abstractions.Auditing;
using Payment.Application.DTOs;
using Payment.Application.DTOs.Audit;
using Payment.Application.Errors;
using Payment.Application.Interfaces;
using Payment.Domain.Entities;

namespace Payment.Application.Features.Orders.UpdateOrderStatus;

public class UpdateOrderStatusCommandHandler : ICommandHandler<UpdateOrderStatusCommand, OrderDto>
{
    private readonly IOrderRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateOrderStatusCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditPublisher _auditPublisher;

    public UpdateOrderStatusCommandHandler(
        IOrderRepository repository,
        IMapper mapper,
        ILogger<UpdateOrderStatusCommandHandler> logger,
        IUnitOfWork unitOfWork,
        IAuditPublisher auditPublisher)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
        _unitOfWork = unitOfWork;
        _auditPublisher = auditPublisher;
    }

    public async Task<Result<OrderDto>> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await _repository.GetByIdAsync(request.OrderId);
        if (order == null)
        {
            return Result.Failure<OrderDto>(PaymentErrors.Order.NotFoundById(request.OrderId));
        }

        var oldOrderData = OrderAuditData.FromOrder(order);

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

        await _auditPublisher.PublishAsync(
            updated.Id,
            OrderAuditData.FromOrder(updated),
            AuditAction.Updated,
            oldOrderData,
            new Dictionary<string, object> { ["Action"] = "StatusUpdated" },
            cancellationToken);

        _logger.LogInformation("Updated order {OrderId}", updated.Id);

        return updated.ToDto(_mapper);
    }
}
