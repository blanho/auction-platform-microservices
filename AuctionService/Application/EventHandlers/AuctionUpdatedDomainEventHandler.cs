using AuctionService.Domain.Events;
using Common.Messaging.Abstractions;
using Common.Messaging.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuctionService.Application.EventHandlers;

public class AuctionUpdatedDomainEventHandler : INotificationHandler<AuctionUpdatedDomainEvent>
{
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<AuctionUpdatedDomainEventHandler> _logger;

    public AuctionUpdatedDomainEventHandler(
        IEventPublisher eventPublisher,
        ILogger<AuctionUpdatedDomainEventHandler> logger)
    {
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task Handle(AuctionUpdatedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Auction {AuctionId} updated by Seller {SellerId}",
            notification.AuctionId,
            notification.SellerId);

        await _eventPublisher.PublishAsync(new AuctionUpdatedEvent
        {
            Id = notification.AuctionId,
            SellerId = notification.SellerId,
            Title = notification.Title
        }, cancellationToken);
    }
}
