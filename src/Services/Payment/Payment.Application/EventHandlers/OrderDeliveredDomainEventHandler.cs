using MediatR;
using Microsoft.Extensions.Logging;
using Payment.Domain.Events;

namespace Payment.Application.EventHandlers;

public class OrderDeliveredDomainEventHandler : INotificationHandler<OrderDeliveredDomainEvent>
{
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<OrderDeliveredDomainEventHandler> _logger;

    public OrderDeliveredDomainEventHandler(
        IEventPublisher eventPublisher,
        ILogger<OrderDeliveredDomainEventHandler> logger)
    {
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task Handle(OrderDeliveredDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing OrderDeliveredDomainEvent for Order {OrderId}",
            notification.OrderId);

        await _eventPublisher.PublishAsync(new OrderDeliveredEvent
        {
            OrderId = notification.OrderId,
            AuctionId = notification.AuctionId,
            BuyerId = notification.BuyerId,
            BuyerUsername = notification.BuyerUsername,
            SellerId = notification.SellerId,
            SellerUsername = notification.SellerUsername,
            DeliveredAt = notification.OccurredAt
        }, cancellationToken);
    }
}
