using BidService.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BidService.Application.EventHandlers;

public class BidAcceptedDomainEventHandler : INotificationHandler<BidAcceptedDomainEvent>
{
    private readonly ILogger<BidAcceptedDomainEventHandler> _logger;

    public BidAcceptedDomainEventHandler(ILogger<BidAcceptedDomainEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(BidAcceptedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Bid {BidId} accepted for Auction {AuctionId}, Amount: {Amount}",
            notification.BidId,
            notification.AuctionId,
            notification.Amount);

        return Task.CompletedTask;
    }
}
