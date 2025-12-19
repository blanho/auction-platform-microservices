using AuctionService.Domain.Events;
using Common.Messaging.Abstractions;
using Common.Messaging.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuctionService.Application.EventHandlers;

public class BuyNowExecutedDomainEventHandler : INotificationHandler<BuyNowExecutedDomainEvent>
{
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<BuyNowExecutedDomainEventHandler> _logger;

    public BuyNowExecutedDomainEventHandler(
        IEventPublisher eventPublisher,
        ILogger<BuyNowExecutedDomainEventHandler> logger)
    {
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task Handle(BuyNowExecutedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Buy Now executed for Auction {AuctionId}. Buyer: {BuyerUsername}, Price: {BuyNowPrice}",
            notification.AuctionId,
            notification.BuyerUsername,
            notification.BuyNowPrice);

        await _eventPublisher.PublishAsync(new BuyNowExecutedEvent
        {
            AuctionId = notification.AuctionId,
            Buyer = notification.BuyerUsername,
            Seller = notification.SellerUsername,
            BuyNowPrice = notification.BuyNowPrice,
            ItemTitle = notification.ItemTitle,
            ExecutedAt = notification.OccurredAt
        }, cancellationToken);
    }
}
