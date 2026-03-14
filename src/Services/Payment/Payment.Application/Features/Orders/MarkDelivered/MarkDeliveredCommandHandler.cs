using AutoMapper;
using BuildingBlocks.Application.Abstractions.Auditing;
using Payment.Application.DTOs;
using Payment.Application.DTOs.Audit;
using Payment.Application.Errors;
using Payment.Application.Interfaces;
using Payment.Domain.Entities;

namespace Payment.Application.Features.Orders.MarkDelivered;

public class MarkDeliveredCommandHandler : ICommandHandler<MarkDeliveredCommand, OrderDto>
{
    private readonly IOrderRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<MarkDeliveredCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditPublisher _auditPublisher;

    public MarkDeliveredCommandHandler(
        IOrderRepository repository,
        IMapper mapper,
        ILogger<MarkDeliveredCommandHandler> logger,
        IUnitOfWork unitOfWork,
        IAuditPublisher auditPublisher)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
        _unitOfWork = unitOfWork;
        _auditPublisher = auditPublisher;
    }

    public async Task<Result<OrderDto>> Handle(MarkDeliveredCommand request, CancellationToken cancellationToken)
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

        if (order.Status == OrderStatus.Delivered)
        {
            return Result.Failure<OrderDto>(PaymentErrors.Order.AlreadyDelivered);
        }

        if (order.Status != OrderStatus.Shipped)
        {
            return Result.Failure<OrderDto>(PaymentErrors.Order.NotShipped);
        }

        _logger.LogInformation("Marking order {OrderId} as delivered", request.OrderId);

        var oldOrderData = OrderAuditData.FromOrder(order);

        order.MarkAsDelivered();

        var updated = await _repository.UpdateAsync(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditPublisher.PublishAsync(
            updated.Id,
            OrderAuditData.FromOrder(updated),
            AuditAction.Updated,
            oldOrderData,
            new Dictionary<string, object> { ["Action"] = "Delivered" },
            cancellationToken);

        _logger.LogInformation("Order {OrderId} marked as delivered", updated.Id);

        return _mapper.Map<OrderDto>(updated);
    }
}
