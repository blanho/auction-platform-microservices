using BidService.Application.Interfaces;
using BidService.Domain.Events;
using Common.Messaging.Abstractions;
using Common.Messaging.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BidService.Application.EventHandlers;

public class HighestBidUpdatedDomainEventHandler : INotificationHandler<HighestBidUpdatedDomainEvent>
{
    private readonly IEventPublisher _eventPublisher;
    private readonly IAutoBidService _autoBidService;
    private readonly ILogger<HighestBidUpdatedDomainEventHandler> _logger;

    public HighestBidUpdatedDomainEventHandler(
        IEventPublisher eventPublisher,
        IAutoBidService autoBidService,
        ILogger<HighestBidUpdatedDomainEventHandler> logger)
    {
        _eventPublisher = eventPublisher;
        _autoBidService = autoBidService;
        _logger = logger;
    }

    public async Task Handle(HighestBidUpdatedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Highest bid updated for Auction {AuctionId}: {NewAmount} by {BidderUsername}",
            notification.AuctionId,
            notification.NewHighestAmount,
            notification.BidderUsername);

        await _eventPublisher.PublishAsync(new BidPlacedEvent
        {
            Id = notification.BidId,
            AuctionId = notification.AuctionId,
            BidderId = notification.BidderId,
            Bidder = notification.BidderUsername,
            BidAmount = notification.NewHighestAmount,
            BidTime = DateTimeOffset.UtcNow,
            BidStatus = "Accepted"
        }, cancellationToken);

        if (notification.PreviousBidderId.HasValue && 
            !string.IsNullOrEmpty(notification.PreviousBidderUsername) &&
            notification.PreviousBidderId != notification.BidderId)
        {
            _logger.LogInformation(
                "Publishing OutbidEvent for previous bidder {PreviousBidder} on auction {AuctionId}",
                notification.PreviousBidderUsername,
                notification.AuctionId);

            await _eventPublisher.PublishAsync(new OutbidEvent
            {
                AuctionId = notification.AuctionId,
                OutbidBidderId = notification.PreviousBidderId.Value,
                OutbidBidderUsername = notification.PreviousBidderUsername,
                NewHighBidderId = notification.BidderId,
                NewHighBidderUsername = notification.BidderUsername,
                NewHighBidAmount = notification.NewHighestAmount,
                PreviousBidAmount = notification.PreviousHighestAmount ?? 0,
                OutbidAt = DateTimeOffset.UtcNow
            }, cancellationToken);
        }
        try
        {
            await _autoBidService.ProcessAutoBidsForAuctionAsync(
                notification.AuctionId,
                notification.NewHighestAmount,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to process auto-bids for auction {AuctionId} after bid update to {Amount}",
                notification.AuctionId, notification.NewHighestAmount);
        }
    }
}
