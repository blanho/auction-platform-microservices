using BidService.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BidService.Application.EventHandlers;

public class BidPlacedDomainEventHandler : INotificationHandler<BidPlacedDomainEvent>
{
    private readonly ILogger<BidPlacedDomainEventHandler> _logger;

    public BidPlacedDomainEventHandler(ILogger<BidPlacedDomainEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(BidPlacedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Bid {BidId} placed on Auction {AuctionId} by {Bidder} for {Amount}",
            notification.BidId,
            notification.AuctionId,
            notification.BidderUsername,
            notification.Amount);

        return Task.CompletedTask;
    }
}
