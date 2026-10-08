using OrchestrationService.Contracts.Events;
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
            OrderCreationManaged = true,
            IsBuyNow = notification.IsBuyNow,
            AuctionId = notification.AuctionId,
            SellerId = notification.SellerId,
            SellerUsername = notification.SellerUsername,
            WinnerId = notification.WinnerId,
            WinnerUsername = notification.WinnerUsername,
            SoldAmount = notification.SoldAmount,
            ItemSold = notification.ItemSold,
            ItemTitle = notification.ItemTitle
        }, cancellationToken);
        if (notification.ItemSold && !notification.IsBuyNow && notification.WinnerId.HasValue)
        {
            await _eventPublisher.PublishAsync(new AuctionCompletionSagaStarted
            {
                CorrelationId = notification.AuctionId,
                AuctionId = notification.AuctionId,
                WinnerId = notification.WinnerId.Value,
                WinnerUsername = notification.WinnerUsername ?? string.Empty,
                SellerId = notification.SellerId,
                SellerUsername = notification.SellerUsername,
                WinningBidAmount = notification.SoldAmount ?? 0,
                ItemTitle = notification.ItemTitle,
                AuctionEndedAt = DateTimeOffset.UtcNow
            }, cancellationToken);
        }
    }
}

