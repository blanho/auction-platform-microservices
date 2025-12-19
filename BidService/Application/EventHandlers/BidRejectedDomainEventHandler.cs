using BidService.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BidService.Application.EventHandlers;

public class BidRejectedDomainEventHandler : INotificationHandler<BidRejectedDomainEvent>
{
    private readonly ILogger<BidRejectedDomainEventHandler> _logger;

    public BidRejectedDomainEventHandler(ILogger<BidRejectedDomainEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(BidRejectedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogWarning(
            "Bid {BidId} rejected for Auction {AuctionId}, Reason: {Reason}",
            notification.BidId,
            notification.AuctionId,
            notification.Reason);

        return Task.CompletedTask;
    }
}
