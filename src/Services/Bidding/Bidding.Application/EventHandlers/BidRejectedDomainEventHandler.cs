using BidService.Contracts.Events;
using Bidding.Domain.Events;
using BuildingBlocks.Application.Abstractions.Messaging;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bidding.Application.EventHandlers;

public class BidRejectedDomainEventHandler : INotificationHandler<BidRejectedDomainEvent>
{
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<BidRejectedDomainEventHandler> _logger;

    public BidRejectedDomainEventHandler(
        IEventPublisher eventPublisher,
        ILogger<BidRejectedDomainEventHandler> logger)
    {
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task Handle(BidRejectedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogWarning(
            "Bid {BidId} rejected for Auction {AuctionId}, Reason: {Reason}",
            notification.BidId,
            notification.AuctionId,
            notification.Reason);

        await _eventPublisher.PublishAsync(new BidRejectedEvent
        {
            BidId = notification.BidId,
            AuctionId = notification.AuctionId,
            BidderId = notification.BidderId,
            BidderUsername = notification.BidderUsername,
            Amount = notification.Amount,
            Reason = notification.Reason,
            RejectedAt = DateTimeOffset.UtcNow
        }, cancellationToken);
    }
}

