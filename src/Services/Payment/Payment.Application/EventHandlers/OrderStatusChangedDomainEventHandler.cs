using MediatR;
using Microsoft.Extensions.Logging;
using Payment.Domain.Events;

namespace Payment.Application.EventHandlers;

public class OrderStatusChangedDomainEventHandler : INotificationHandler<OrderStatusChangedDomainEvent>
{
    private readonly ILogger<OrderStatusChangedDomainEventHandler> _logger;

    public OrderStatusChangedDomainEventHandler(ILogger<OrderStatusChangedDomainEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(OrderStatusChangedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Order {OrderId} status changed from {OldStatus} to {NewStatus}",
            notification.OrderId,
            notification.OldStatus,
            notification.NewStatus);

        return Task.CompletedTask;
    }
}
