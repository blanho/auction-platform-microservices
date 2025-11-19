using MediatR;
using Microsoft.Extensions.Logging;
using Payment.Domain.Events;

namespace Payment.Application.EventHandlers;

public class OrderCreatedDomainEventHandler : INotificationHandler<OrderCreatedDomainEvent>
{
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<OrderCreatedDomainEventHandler> _logger;

    public OrderCreatedDomainEventHandler(
        IEventPublisher eventPublisher,
        ILogger<OrderCreatedDomainEventHandler> logger)
    {
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task Handle(OrderCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing OrderCreatedDomainEvent for Order {OrderId}, Auction {AuctionId}",
            notification.OrderId,
            notification.AuctionId);

        await _eventPublisher.PublishAsync(new OrderCreatedEvent
        {
            OrderId = notification.OrderId,
            AuctionId = notification.AuctionId,
            BuyerId = notification.BuyerId,
            BuyerUsername = notification.BuyerUsername,
            SellerId = notification.SellerId,
            SellerUsername = notification.SellerUsername,
            ItemTitle = notification.ItemTitle,
            TotalAmount = notification.TotalAmount,
            CreatedAt = notification.OccurredAt
        }, cancellationToken);
    }
}
