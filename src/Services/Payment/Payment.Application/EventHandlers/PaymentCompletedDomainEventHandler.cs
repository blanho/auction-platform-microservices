using MediatR;
using Microsoft.Extensions.Logging;
using Payment.Domain.Events;

namespace Payment.Application.EventHandlers;

public class PaymentCompletedDomainEventHandler : INotificationHandler<PaymentCompletedDomainEvent>
{
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<PaymentCompletedDomainEventHandler> _logger;

    public PaymentCompletedDomainEventHandler(
        IEventPublisher eventPublisher,
        ILogger<PaymentCompletedDomainEventHandler> logger)
    {
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task Handle(PaymentCompletedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing PaymentCompletedDomainEvent for Order {OrderId}, Amount: {Amount}",
            notification.OrderId,
            notification.Amount);

        await _eventPublisher.PublishAsync(new PaymentCompletedEvent
        {
            OrderId = notification.OrderId,
            AuctionId = notification.AuctionId,
            BuyerId = notification.BuyerId,
            BuyerUsername = notification.BuyerUsername,
            SellerId = notification.SellerId,
            SellerUsername = notification.SellerUsername,
            Amount = notification.Amount,
            TransactionId = notification.TransactionId,
            PaidAt = notification.OccurredAt
        }, cancellationToken);
    }
}
