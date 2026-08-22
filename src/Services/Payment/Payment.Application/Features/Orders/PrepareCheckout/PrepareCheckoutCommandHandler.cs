using AutoMapper;
using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Application.Constants;
using Payment.Application.DTOs;
using Payment.Application.DTOs.Audit;
using Payment.Application.Errors;
using Payment.Application.Interfaces;
using Payment.Domain.Constants;

namespace Payment.Application.Features.Orders.PrepareCheckout;

public class PrepareCheckoutCommandHandler : ICommandHandler<PrepareCheckoutCommand, OrderDto>
{
    private readonly IOrderRepository _repository;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditPublisher _auditPublisher;
    private readonly ILogger<PrepareCheckoutCommandHandler> _logger;

    public PrepareCheckoutCommandHandler(
        IOrderRepository repository,
        IMapper mapper,
        IUnitOfWork unitOfWork,
        IAuditPublisher auditPublisher,
        ILogger<PrepareCheckoutCommandHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _auditPublisher = auditPublisher;
        _logger = logger;
    }

    public async Task<Result<OrderDto>> Handle(
        PrepareCheckoutCommand request,
        CancellationToken cancellationToken)
    {
        var order = await _repository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null || order.BuyerId != request.BuyerId)
        {
            return Result.Failure<OrderDto>(PaymentErrors.Order.NotFoundById(request.OrderId));
        }

        if (order.PaymentStatus != PaymentStatus.Pending ||
            order.Status is not (OrderStatus.Pending or OrderStatus.PaymentPending))
        {
            return Result.Failure<OrderDto>(
                PaymentErrors.Order.InvalidStatusWithDetails(order.Status.ToString()));
        }

        var oldOrderData = OrderAuditData.FromOrder(order);

        order.SetShippingAddress(request.ShippingAddress);
        if (!string.IsNullOrWhiteSpace(request.BuyerNotes))
        {
            order.AddBuyerNotes(request.BuyerNotes);
        }

        var updated = await _repository.UpdateAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditPublisher.PublishAsync(
            updated.Id,
            OrderAuditData.FromOrder(updated),
            AuditAction.Updated,
            oldOrderData,
            new Dictionary<string, object>
            {
                [AuditMetadataKeys.Action] = OrderAuditActions.CheckoutPrepared
            },
            cancellationToken);

        _logger.LogInformation("Checkout details prepared for order {OrderId}", updated.Id);

        return updated.ToDto(_mapper);
    }
}
