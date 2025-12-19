using AuctionService.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuctionService.Application.EventHandlers;

public class AuctionStatusChangedDomainEventHandler : INotificationHandler<AuctionStatusChangedDomainEvent>
{
    private readonly ILogger<AuctionStatusChangedDomainEventHandler> _logger;

    public AuctionStatusChangedDomainEventHandler(ILogger<AuctionStatusChangedDomainEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(AuctionStatusChangedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Auction {AuctionId} status changed from {OldStatus} to {NewStatus}",
            notification.AuctionId,
            notification.OldStatus,
            notification.NewStatus);

        return Task.CompletedTask;
    }
}
