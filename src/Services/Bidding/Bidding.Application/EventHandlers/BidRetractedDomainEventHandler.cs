using BidService.Contracts.Events;
using Bidding.Domain.Events;
using BuildingBlocks.Application.Abstractions.Messaging;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bidding.Application.EventHandlers;

public class BidRetractedDomainEventHandler : INotificationHandler<BidRetractedDomainEvent>
{
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<BidRetractedDomainEventHandler> _logger;

    public BidRetractedDomainEventHandler(
        IEventPublisher eventPublisher,
        ILogger<BidRetractedDomainEventHandler> logger)
    {
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task Handle(BidRetractedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Bid {BidId} retracted for Auction {AuctionId}, WasHighest: {WasHighest}",
            notification.BidId,
            notification.AuctionId,
            notification.WasHighestBid);

        await _eventPublisher.PublishAsync(new BidRetractedEvent
        {
            BidId = notification.BidId,
            AuctionId = notification.AuctionId,
            BidderId = notification.BidderId,
            Bidder = notification.BidderUsername,
            BidAmount = notification.Amount,
            RetractedAmount = notification.Amount,
            Reason = notification.Reason,
            RetractedAt = DateTimeOffset.UtcNow,
            NewHighestBidId = notification.NewHighestBidId,
            NewHighestAmount = notification.NewHighestAmount,
            NewHighestBidderId = notification.NewHighestBidderId,
            NewHighestBidderUsername = notification.NewHighestBidderUsername
        }, cancellationToken);
    }
}
