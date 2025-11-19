using Auctions.Domain.Events;
using BuildingBlocks.Application.Abstractions.Messaging;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Auctions.Application.EventHandlers;

public class AuctionCreatedDomainEventHandler : INotificationHandler<AuctionCreatedDomainEvent>
{
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<AuctionCreatedDomainEventHandler> _logger;

    public AuctionCreatedDomainEventHandler(
        IEventPublisher eventPublisher,
        ILogger<AuctionCreatedDomainEventHandler> logger)
    {
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task Handle(AuctionCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing AuctionCreatedDomainEvent for Auction {AuctionId} by Seller {SellerUsername}",
            notification.AuctionId,
            notification.SellerUsername);

        await _eventPublisher.PublishAsync(new AuctionCreatedEvent
        {
            Id = notification.AuctionId,
            SellerId = notification.SellerId,
            SellerUsername = notification.SellerUsername,
            ReservePrice = notification.ReservePrice,
            AuctionEnd = notification.AuctionEnd,
            CreatedAt = notification.OccurredAt
        }, cancellationToken);
    }
}

