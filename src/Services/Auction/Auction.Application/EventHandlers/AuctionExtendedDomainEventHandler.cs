using Auctions.Domain.Events;
using BuildingBlocks.Application.Abstractions.Messaging;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Auctions.Application.EventHandlers;

public class AuctionExtendedDomainEventHandler : INotificationHandler<AuctionExtendedDomainEvent>
{
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<AuctionExtendedDomainEventHandler> _logger;

    public AuctionExtendedDomainEventHandler(
        IEventPublisher eventPublisher,
        ILogger<AuctionExtendedDomainEventHandler> logger)
    {
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task Handle(AuctionExtendedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Auction {AuctionId} extended to {NewEndTime} (extension #{Times}). Reason: {Reason}",
            notification.AuctionId,
            notification.NewEndTime,
            notification.TimesExtended,
            notification.Reason);

        await _eventPublisher.PublishAsync(new AuctionExtendedEvent
        {
            AuctionId = notification.AuctionId,
            OldEndTime = notification.OldEndTime,
            NewEndTime = notification.NewEndTime,
            TimesExtended = notification.TimesExtended,
            Reason = notification.Reason
        }, cancellationToken);
    }
}

