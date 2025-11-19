using Auctions.Domain.Events;
using BuildingBlocks.Application.Abstractions.Messaging;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Auctions.Application.EventHandlers;

public class AuctionDeletedDomainEventHandler : INotificationHandler<AuctionDeletedDomainEvent>
{
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<AuctionDeletedDomainEventHandler> _logger;

    public AuctionDeletedDomainEventHandler(
        IEventPublisher eventPublisher,
        ILogger<AuctionDeletedDomainEventHandler> logger)
    {
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task Handle(AuctionDeletedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Auction {AuctionId} deleted by Seller {SellerId}",
            notification.AuctionId,
            notification.SellerId);

        await _eventPublisher.PublishAsync(new AuctionDeletedEvent
        {
            Id = notification.AuctionId
        }, cancellationToken);
    }
}

