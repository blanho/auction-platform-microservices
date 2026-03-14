namespace Bidding.Application.EventHandlers;

public class AutoBidActivatedDomainEventHandler : INotificationHandler<AutoBidActivatedDomainEvent>
{
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<AutoBidActivatedDomainEventHandler> _logger;

    public AutoBidActivatedDomainEventHandler(
        IEventPublisher eventPublisher,
        ILogger<AutoBidActivatedDomainEventHandler> logger)
    {
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task Handle(AutoBidActivatedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "AutoBid {AutoBidId} activated for Auction {AuctionId} by User {UserId}",
            notification.AutoBidId,
            notification.AuctionId,
            notification.UserId);

        await _eventPublisher.PublishAsync(new AutoBidActivatedEvent
        {
            AutoBidId = notification.AutoBidId,
            AuctionId = notification.AuctionId,
            UserId = notification.UserId,
            ActivatedAt = notification.OccurredAt
        }, cancellationToken);
    }
}
