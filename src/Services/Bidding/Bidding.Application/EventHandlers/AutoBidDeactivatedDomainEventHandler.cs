namespace Bidding.Application.EventHandlers;

public class AutoBidDeactivatedDomainEventHandler : INotificationHandler<AutoBidDeactivatedDomainEvent>
{
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<AutoBidDeactivatedDomainEventHandler> _logger;

    public AutoBidDeactivatedDomainEventHandler(
        IEventPublisher eventPublisher,
        ILogger<AutoBidDeactivatedDomainEventHandler> logger)
    {
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task Handle(AutoBidDeactivatedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "AutoBid {AutoBidId} deactivated for Auction {AuctionId} by User {UserId}",
            notification.AutoBidId,
            notification.AuctionId,
            notification.UserId);

        await _eventPublisher.PublishAsync(new AutoBidDeactivatedEvent
        {
            AutoBidId = notification.AutoBidId,
            AuctionId = notification.AuctionId,
            UserId = notification.UserId,
            DeactivatedAt = notification.OccurredAt
        }, cancellationToken);
    }
}
