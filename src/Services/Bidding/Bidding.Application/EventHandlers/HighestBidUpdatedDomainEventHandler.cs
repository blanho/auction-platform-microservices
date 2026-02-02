using MediatR;
using Microsoft.Extensions.Logging;
using BidService.Contracts.Events;

namespace Bidding.Application.EventHandlers;

public class HighestBidUpdatedDomainEventHandler : INotificationHandler<HighestBidUpdatedDomainEvent>
{
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<HighestBidUpdatedDomainEventHandler> _logger;

    public HighestBidUpdatedDomainEventHandler(
        IEventPublisher eventPublisher,
        ILogger<HighestBidUpdatedDomainEventHandler> logger)
    {
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task Handle(HighestBidUpdatedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Highest bid updated for Auction {AuctionId}: {NewAmount}",
            notification.AuctionId,
            notification.NewHighestAmount);

        await _eventPublisher.PublishAsync(new BidPlacedEvent
        {
            Id = notification.BidId,
            AuctionId = notification.AuctionId,
            BidderId = notification.BidderId,
            Bidder = notification.BidderUsername,
            BidTime = DateTimeOffset.UtcNow,
            BidAmount = notification.NewHighestAmount,
            BidStatus = "Accepted"
        }, cancellationToken);

        if (notification.PreviousBidderId.HasValue &&
            !string.IsNullOrEmpty(notification.PreviousBidderUsername) &&
            notification.PreviousBidderId != notification.BidderId)
        {
            _logger.LogDebug(
                "Publishing OutbidEvent for auction {AuctionId}",
                notification.AuctionId);

            await _eventPublisher.PublishAsync(new OutbidEvent
            {
                AuctionId = notification.AuctionId,
                OutbidBidderId = notification.PreviousBidderId.Value,
                OutbidBidderUsername = notification.PreviousBidderUsername ?? string.Empty,
                NewHighBidderId = notification.BidderId,
                NewHighBidderUsername = notification.BidderUsername,
                NewHighBidAmount = notification.NewHighestAmount,
                PreviousBidAmount = notification.PreviousHighestAmount ?? 0,
                OutbidAt = DateTimeOffset.UtcNow
            }, cancellationToken);
        }

        if (notification.IsAutoBid)
        {
            _logger.LogDebug(
                "Skipping auto-bid trigger for auto-bid on auction {AuctionId}",
                notification.AuctionId);
            return;
        }

        await _eventPublisher.PublishAsync(new ProcessAutoBidsEvent
        {
            AuctionId = notification.AuctionId,
            CurrentHighBid = notification.NewHighestAmount,
            CurrentHighBidderId = notification.BidderId,
            CurrentHighBidderUsername = notification.BidderUsername,
            TriggeredAt = DateTimeOffset.UtcNow
        }, cancellationToken);

        _logger.LogDebug(
            "Published ProcessAutoBidsEvent for auction {AuctionId} at bid amount {Amount}",
            notification.AuctionId, notification.NewHighestAmount);
    }
}
