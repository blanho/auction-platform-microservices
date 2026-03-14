using AutoMapper;
using BuildingBlocks.Application.Abstractions.Auditing;
using Payment.Application.DTOs;
using Payment.Application.DTOs.Audit;
using Payment.Application.Errors;
using Payment.Application.Interfaces;
using Payment.Domain.Entities;

namespace Payment.Application.Features.Orders.ProcessPayment;

public class ProcessPaymentCommandHandler : ICommandHandler<ProcessPaymentCommand, OrderDto>
{
    private readonly IOrderRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<ProcessPaymentCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditPublisher _auditPublisher;

    public ProcessPaymentCommandHandler(
        IOrderRepository repository,
        IMapper mapper,
        ILogger<ProcessPaymentCommandHandler> logger,
        IUnitOfWork unitOfWork,
        IAuditPublisher auditPublisher)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
        _unitOfWork = unitOfWork;
        _auditPublisher = auditPublisher;
    }

    public async Task<Result<OrderDto>> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        var order = await _repository.GetByIdAsync(request.OrderId);
        if (order == null)
        {
            return Result.Failure<OrderDto>(PaymentErrors.Order.NotFoundById(request.OrderId));
        }

        if (order.PaymentStatus == PaymentStatus.Completed)
        {
            return Result.Failure<OrderDto>(PaymentErrors.Order.AlreadyPaidById(request.OrderId));
        }

        if (order.Status == OrderStatus.Cancelled)
        {
            return Result.Failure<OrderDto>(PaymentErrors.Order.CancelledById(request.OrderId));
        }

        _logger.LogInformation("Processing payment for order {OrderId} via {PaymentMethod}", request.OrderId, request.PaymentMethod);

        var oldOrderData = OrderAuditData.FromOrder(order);

        order.CompletePayment(request.ExternalTransactionId);

        var updated = await _repository.UpdateAsync(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditPublisher.PublishAsync(
            updated.Id,
            OrderAuditData.FromOrder(updated),
            AuditAction.Updated,
            oldOrderData,
            new Dictionary<string, object>
            {
                ["Action"] = "PaymentCompleted",
                ["PaymentMethod"] = request.PaymentMethod ?? "Unknown",
                ["TransactionId"] = request.ExternalTransactionId ?? string.Empty
            },
            cancellationToken);

        _logger.LogInformation("Payment completed for order {OrderId}", updated.Id);

        return updated.ToDto(_mapper);
    }
}
