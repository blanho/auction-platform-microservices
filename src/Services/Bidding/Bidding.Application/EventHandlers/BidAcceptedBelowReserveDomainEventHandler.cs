namespace Bidding.Application.EventHandlers;

public class BidAcceptedBelowReserveDomainEventHandler : INotificationHandler<BidAcceptedBelowReserveDomainEvent>
{
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<BidAcceptedBelowReserveDomainEventHandler> _logger;

    public BidAcceptedBelowReserveDomainEventHandler(
        IEventPublisher eventPublisher,
        ILogger<BidAcceptedBelowReserveDomainEventHandler> logger)
    {
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task Handle(BidAcceptedBelowReserveDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Bid {BidId} accepted below reserve for Auction {AuctionId}, Amount: {Amount}",
            notification.BidId,
            notification.AuctionId,
            notification.Amount);

        await _eventPublisher.PublishAsync(new BidAcceptedBelowReserveEvent
        {
            BidId = notification.BidId,
            AuctionId = notification.AuctionId,
            BidderId = notification.BidderId,
            BidderUsername = notification.BidderUsername,
            Amount = notification.Amount,
            AcceptedAt = notification.OccurredAt
        }, cancellationToken);
    }
}
