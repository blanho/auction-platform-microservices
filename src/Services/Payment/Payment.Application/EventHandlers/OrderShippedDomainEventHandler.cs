using MediatR;
using Microsoft.Extensions.Logging;
using Payment.Domain.Events;

namespace Payment.Application.EventHandlers;

public class OrderShippedDomainEventHandler : INotificationHandler<OrderShippedDomainEvent>
{
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<OrderShippedDomainEventHandler> _logger;

    public OrderShippedDomainEventHandler(
        IEventPublisher eventPublisher,
        ILogger<OrderShippedDomainEventHandler> logger)
    {
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task Handle(OrderShippedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing OrderShippedDomainEvent for Order {OrderId}, Tracking: {TrackingNumber}",
            notification.OrderId,
            notification.TrackingNumber);

        await _eventPublisher.PublishAsync(new OrderShippedEvent
        {
            OrderId = notification.OrderId,
            AuctionId = notification.AuctionId,
            BuyerId = notification.BuyerId,
            BuyerUsername = notification.BuyerUsername,
            TrackingNumber = notification.TrackingNumber,
            ShippingCarrier = notification.ShippingCarrier,
            ShippedAt = notification.OccurredAt
        }, cancellationToken);
    }
}
