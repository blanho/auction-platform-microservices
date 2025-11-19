using BidService.Contracts.Events;
using Bidding.Domain.Events;
using BuildingBlocks.Application.Abstractions.Messaging;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bidding.Application.EventHandlers;

public class BidAcceptedDomainEventHandler : INotificationHandler<BidAcceptedDomainEvent>
{
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<BidAcceptedDomainEventHandler> _logger;

    public BidAcceptedDomainEventHandler(
        IEventPublisher eventPublisher,
        ILogger<BidAcceptedDomainEventHandler> logger)
    {
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task Handle(BidAcceptedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Bid {BidId} accepted for Auction {AuctionId}, Amount: {Amount}",
            notification.BidId,
            notification.AuctionId,
            notification.Amount);

        await _eventPublisher.PublishAsync(new BidAcceptedEvent
        {
            BidId = notification.BidId,
            AuctionId = notification.AuctionId,
            BidderId = notification.BidderId,
            BidderUsername = string.Empty,
            Amount = notification.Amount,
            AcceptedAt = DateTimeOffset.UtcNow
        }, cancellationToken);
    }
}

