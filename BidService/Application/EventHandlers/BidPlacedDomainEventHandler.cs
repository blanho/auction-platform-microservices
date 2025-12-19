using BidService.Domain.Events;
using Common.Messaging.Abstractions;
using Common.Messaging.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BidService.Application.EventHandlers;

public class BidPlacedDomainEventHandler : INotificationHandler<BidPlacedDomainEvent>
{
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<BidPlacedDomainEventHandler> _logger;

    public BidPlacedDomainEventHandler(
        IEventPublisher eventPublisher,
        ILogger<BidPlacedDomainEventHandler> logger)
    {
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task Handle(BidPlacedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing BidPlacedDomainEvent for Bid {BidId} on Auction {AuctionId}",
            notification.BidId,
            notification.AuctionId);

        await _eventPublisher.PublishAsync(new BidPlacedEvent
        {
            Id = notification.BidId,
            AuctionId = notification.AuctionId,
            Bidder = notification.BidderUsername,
            BidAmount = notification.Amount,
            BidTime = notification.BidTime
        }, cancellationToken);
    }
}
