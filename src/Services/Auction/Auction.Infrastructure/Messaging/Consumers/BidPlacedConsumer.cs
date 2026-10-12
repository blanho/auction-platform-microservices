namespace Auctions.Infrastructure.Messaging.Consumers;

public class BidPlacedConsumer : IConsumer<BidPlacedEvent>, IConsumer<HighestBidUpdatedEvent>
{
    private readonly IAuctionWriteRepository _writeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BidPlacedConsumer> _logger;

    public BidPlacedConsumer(
        IAuctionWriteRepository writeRepository,
        IUnitOfWork unitOfWork,
        ILogger<BidPlacedConsumer> logger)
    {
        _writeRepository = writeRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public Task Consume(ConsumeContext<HighestBidUpdatedEvent> context)
    {
        var message = context.Message;
        return ApplyAcceptedBidAsync(message.Id, message.AuctionId, message.NewHighestAmount,
            message.BidderId, message.BidderUsername, message.BidStatus, context.CancellationToken);
    }

    public Task Consume(ConsumeContext<BidPlacedEvent> context)
    {
        var message = context.Message;
        return ApplyAcceptedBidAsync(message.Id, message.AuctionId, message.BidAmount,
            message.BidderId, message.Bidder, message.BidStatus, context.CancellationToken);
    }

    private async Task ApplyAcceptedBidAsync(
        Guid eventId,
        Guid auctionId,
        decimal amount,
        Guid bidderId,
        string bidderUsername,
        string status,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing bid update {EventId} for auction {AuctionId}, Amount: {Amount}, Status: {Status}",
            eventId, auctionId, amount, status);

        var auction = await _writeRepository.GetByIdForUpdateAsync(auctionId, cancellationToken);
        if (auction == null)
        {
            _logger.LogWarning("Auction {AuctionId} not found for event {EventId}", auctionId, eventId);
            return;
        }

        if (status is not (
                BidEventStatusNames.Accepted or
                BidEventStatusNames.AcceptedBelowReserve))
        {
            _logger.LogDebug(
                "Bid {EventId} not accepted (Status: {Status}), skipping update",
                eventId, status);
            return;
        }

        if (auction.CurrentHighBid.HasValue && amount <= auction.CurrentHighBid.Value)
        {
            _logger.LogDebug(
                "Bid {EventId} amount {BidAmount} is not higher than current {CurrentBid}, skipping (idempotent)",
                eventId, amount, auction.CurrentHighBid.Value);
            return;
        }

        auction.UpdateHighBid(amount, bidderId, bidderUsername);
        await _writeRepository.UpdateAsync(auction, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Updated auction {AuctionId} high bid to {Amount} by {Bidder}",
            auctionId, amount, bidderUsername);
    }
}
