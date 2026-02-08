using Auctions.Domain.Events;
using Auctions.Domain.Enums;
using BuildingBlocks.Application.Abstractions.Messaging;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Auctions.Application.EventHandlers;

public class AuctionStatusChangedDomainEventHandler : INotificationHandler<AuctionStatusChangedDomainEvent>
{
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<AuctionStatusChangedDomainEventHandler> _logger;

    public AuctionStatusChangedDomainEventHandler(
        IEventPublisher eventPublisher,
        ILogger<AuctionStatusChangedDomainEventHandler> logger)
    {
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task Handle(AuctionStatusChangedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Auction {AuctionId} status changed from {OldStatus} to {NewStatus}",
            notification.AuctionId,
            notification.OldStatus,
            notification.NewStatus);

        if (notification.NewStatus == Status.Live && notification.OldStatus != Status.Live)
        {
            await _eventPublisher.PublishAsync(new AuctionStartedEvent
            {
                AuctionId = notification.AuctionId,
                Seller = notification.SellerUsername,
                Title = notification.Title,
                StartTime = DateTime.UtcNow,
                EndTime = notification.AuctionEnd.DateTime,
                ReservePrice = notification.ReservePrice
            }, cancellationToken);

            _logger.LogInformation("Published AuctionStartedEvent for auction {AuctionId}", notification.AuctionId);
        }
    }
}

