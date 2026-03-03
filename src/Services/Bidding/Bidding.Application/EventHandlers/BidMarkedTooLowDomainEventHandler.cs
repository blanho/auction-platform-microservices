namespace Bidding.Application.EventHandlers;

public class BidMarkedTooLowDomainEventHandler : INotificationHandler<BidMarkedTooLowDomainEvent>
{
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<BidMarkedTooLowDomainEventHandler> _logger;

    public BidMarkedTooLowDomainEventHandler(
        IEventPublisher eventPublisher,
        ILogger<BidMarkedTooLowDomainEventHandler> logger)
    {
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task Handle(BidMarkedTooLowDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogWarning(
            "Bid {BidId} marked too low for Auction {AuctionId}, Amount: {Amount}",
            notification.BidId,
            notification.AuctionId,
            notification.Amount);

        await _eventPublisher.PublishAsync(new BidMarkedTooLowEvent
        {
            BidId = notification.BidId,
            AuctionId = notification.AuctionId,
            BidderId = notification.BidderId,
            Amount = notification.Amount,
            MarkedAt = notification.OccurredAt
        }, cancellationToken);
    }
}
