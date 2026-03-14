namespace Bidding.Application.EventHandlers;

public class AutoBidMaxAmountUpdatedDomainEventHandler : INotificationHandler<AutoBidMaxAmountUpdatedDomainEvent>
{
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<AutoBidMaxAmountUpdatedDomainEventHandler> _logger;

    public AutoBidMaxAmountUpdatedDomainEventHandler(
        IEventPublisher eventPublisher,
        ILogger<AutoBidMaxAmountUpdatedDomainEventHandler> logger)
    {
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task Handle(AutoBidMaxAmountUpdatedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "AutoBid {AutoBidId} max amount updated to {NewMaxAmount} for Auction {AuctionId}",
            notification.AutoBidId,
            notification.NewMaxAmount,
            notification.AuctionId);

        await _eventPublisher.PublishAsync(new AutoBidUpdatedEvent
        {
            AutoBidId = notification.AutoBidId,
            AuctionId = notification.AuctionId,
            UserId = notification.UserId,
            NewMaxAmount = notification.NewMaxAmount,
            UpdatedAt = notification.OccurredAt
        }, cancellationToken);
    }
}
