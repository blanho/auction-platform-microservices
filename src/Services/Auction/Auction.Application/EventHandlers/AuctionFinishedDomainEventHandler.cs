using Auctions.Domain.Events;
using BuildingBlocks.Application.Abstractions.Messaging;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Auctions.Application.EventHandlers;

public class AuctionFinishedDomainEventHandler : INotificationHandler<AuctionFinishedDomainEvent>
{
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<AuctionFinishedDomainEventHandler> _logger;

    public AuctionFinishedDomainEventHandler(
        IEventPublisher eventPublisher,
        ILogger<AuctionFinishedDomainEventHandler> logger)
    {
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task Handle(AuctionFinishedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Auction {AuctionId} finished. ItemSold: {ItemSold}, SoldAmount: {SoldAmount}",
            notification.AuctionId,
            notification.ItemSold,
            notification.SoldAmount);

        await _eventPublisher.PublishAsync(new AuctionFinishedEvent
        {
            AuctionId = notification.AuctionId,
            SellerId = notification.SellerId,
            SellerUsername = notification.SellerUsername,
            WinnerId = notification.WinnerId,
            WinnerUsername = notification.WinnerUsername,
            SoldAmount = notification.SoldAmount,
            ItemSold = notification.ItemSold,
            ItemTitle = notification.ItemTitle
        }, cancellationToken);
    }
}

