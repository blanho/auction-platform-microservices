using AutoMapper;
using Common.Core.Helpers;
using Common.CQRS.Abstractions;
using Common.Repository.Interfaces;
using PaymentService.Application.DTOs;
using PaymentService.Application.Interfaces;
using PaymentService.Domain.Entities;

namespace PaymentService.Application.Commands.ProcessPayment;

public class ProcessPaymentCommandHandler : ICommandHandler<ProcessPaymentCommand, OrderDto>
{
    private readonly IOrderRepository _repository;
    private readonly IMapper _mapper;
    private readonly IAppLogger<ProcessPaymentCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public ProcessPaymentCommandHandler(
        IOrderRepository repository,
        IMapper mapper,
        IAppLogger<ProcessPaymentCommandHandler> logger,
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
            return Result.Failure<OrderDto>(Error.Create("Order.NotFound", $"Order {request.OrderId} not found"));
        }

        if (order.PaymentStatus == PaymentStatus.Completed)
        {
            return Result.Failure<OrderDto>(Error.Create("Order.AlreadyPaid", $"Order {request.OrderId} has already been paid"));
        }

        if (order.Status == OrderStatus.Cancelled)
        {
            return Result.Failure<OrderDto>(Error.Create("Order.Cancelled", $"Order {request.OrderId} has been cancelled"));
        }

        _logger.LogInformation("Processing payment for order {OrderId} via {PaymentMethod}", request.OrderId, request.PaymentMethod);

        order.CompletePayment(request.ExternalTransactionId);

        var updated = await _repository.UpdateAsync(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Payment completed for order {OrderId}", updated.Id);

        return _mapper.Map<OrderDto>(updated);
    }
}
