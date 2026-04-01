using Auctions.Domain.Events;
using Auctions.Domain.Enums;
using BuildingBlocks.Application.Abstractions.Messaging;
using BuildingBlocks.Application.Abstractions.Providers;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Auctions.Application.EventHandlers;

public class AuctionStatusChangedDomainEventHandler : INotificationHandler<AuctionStatusChangedDomainEvent>
{
    private readonly IEventPublisher _eventPublisher;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger<AuctionStatusChangedDomainEventHandler> _logger;

    public AuctionStatusChangedDomainEventHandler(
        IEventPublisher eventPublisher,
        IDateTimeProvider dateTime,
        ILogger<AuctionStatusChangedDomainEventHandler> logger)
    {
        _eventPublisher = eventPublisher;
        _dateTime = dateTime;
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
                StartTime = _dateTime.UtcNowOffset,
                EndTime = notification.AuctionEnd,
                ReservePrice = notification.ReservePrice
            }, cancellationToken);

            _logger.LogInformation("Published AuctionStartedEvent for auction {AuctionId}", notification.AuctionId);
        }
    }
}

