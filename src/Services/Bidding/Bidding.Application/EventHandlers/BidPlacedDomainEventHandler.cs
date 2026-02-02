using BidService.Contracts.Events;
using Bidding.Domain.Events;
using BuildingBlocks.Application.Abstractions.Messaging;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bidding.Application.EventHandlers;

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
        _logger.LogDebug(
            "Bid {BidId} placed on Auction {AuctionId} for {Amount}",
            notification.BidId,
            notification.AuctionId,
            notification.Amount);

        await _eventPublisher.PublishAsync(new BidPlacedEvent
        {
            Id = notification.BidId,
            AuctionId = notification.AuctionId,
            BidderId = notification.BidderId,
            Bidder = notification.BidderUsername,
            BidTime = notification.BidTime,
            BidAmount = notification.Amount,
            BidStatus = "Placed"
        }, cancellationToken);
    }
}

