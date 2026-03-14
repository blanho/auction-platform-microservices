namespace Bidding.Application.EventHandlers;

public class AutoBidCreatedDomainEventHandler : INotificationHandler<AutoBidCreatedDomainEvent>
{
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<AutoBidCreatedDomainEventHandler> _logger;

    public AutoBidCreatedDomainEventHandler(
        IEventPublisher eventPublisher,
        ILogger<AutoBidCreatedDomainEventHandler> logger)
    {
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task Handle(AutoBidCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "AutoBid {AutoBidId} created for Auction {AuctionId} by User {UserId} with MaxAmount {MaxAmount}",
            notification.AutoBidId,
            notification.AuctionId,
            notification.UserId,
            notification.MaxAmount);

        await _eventPublisher.PublishAsync(new AutoBidCreatedEvent
        {
            AutoBidId = notification.AutoBidId,
            AuctionId = notification.AuctionId,
            UserId = notification.UserId,
            Username = notification.Username,
            MaxAmount = notification.MaxAmount,
            CreatedAt = notification.OccurredAt
        }, cancellationToken);
    }
}
