using BidService.Domain.Events;
using Common.Messaging.Abstractions;
using Common.Messaging.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BidService.Application.EventHandlers;

public class HighestBidUpdatedDomainEventHandler : INotificationHandler<HighestBidUpdatedDomainEvent>
{
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<HighestBidUpdatedDomainEventHandler> _logger;

    public HighestBidUpdatedDomainEventHandler(
        IEventPublisher eventPublisher,
        ILogger<HighestBidUpdatedDomainEventHandler> logger)
    {
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task Handle(HighestBidUpdatedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Highest bid updated for Auction {AuctionId}: {NewAmount} by {BidderUsername}",
            notification.AuctionId,
            notification.NewHighestAmount,
            notification.BidderUsername);

        await _eventPublisher.PublishAsync(new BidPlacedEvent
        {
            Id = notification.BidId,
            AuctionId = notification.AuctionId,
            BidderId = notification.BidderId,
            Bidder = notification.BidderUsername,
            BidAmount = notification.NewHighestAmount,
            BidTime = DateTimeOffset.UtcNow,
            BidStatus = "Accepted"
        }, cancellationToken);
    }
}
