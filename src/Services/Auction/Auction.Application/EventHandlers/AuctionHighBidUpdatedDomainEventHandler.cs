using Auctions.Domain.Events;
using BuildingBlocks.Application.Abstractions.Messaging;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Auctions.Application.EventHandlers;

public class AuctionHighBidUpdatedDomainEventHandler : INotificationHandler<AuctionHighBidUpdatedDomainEvent>
{
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<AuctionHighBidUpdatedDomainEventHandler> _logger;

    public AuctionHighBidUpdatedDomainEventHandler(
        IEventPublisher eventPublisher,
        ILogger<AuctionHighBidUpdatedDomainEventHandler> logger)
    {
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task Handle(AuctionHighBidUpdatedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Auction {AuctionId} high bid updated to {BidAmount} by Bidder {BidderId}",
            notification.AuctionId,
            notification.BidAmount,
            notification.BidderId);

        await _eventPublisher.PublishAsync(new AuctionHighBidUpdatedEvent
        {
            AuctionId = notification.AuctionId,
            BidAmount = notification.BidAmount,
            BidderId = notification.BidderId,
            BidderUsername = notification.BidderUsername,
            UpdatedAt = notification.OccurredAt
        }, cancellationToken);
    }
}
