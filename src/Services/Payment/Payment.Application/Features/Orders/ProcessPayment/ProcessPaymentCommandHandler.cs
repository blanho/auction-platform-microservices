using AutoMapper;
using Payment.Application.DTOs;
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

    public ProcessPaymentCommandHandler(
        IOrderRepository repository,
        IMapper mapper,
        ILogger<ProcessPaymentCommandHandler> logger,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
        _unitOfWork = unitOfWork;
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

        order.CompletePayment(request.ExternalTransactionId);

        var updated = await _repository.UpdateAsync(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Payment completed for order {OrderId}", updated.Id);

        return updated.ToDto(_mapper);
    }
}
