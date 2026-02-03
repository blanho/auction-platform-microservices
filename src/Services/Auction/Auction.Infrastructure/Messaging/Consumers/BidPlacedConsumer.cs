using MassTransit;
using Microsoft.Extensions.Logging;

namespace Auctions.Infrastructure.Messaging.Consumers;

/// Consumes BidPlacedEvent to update auction's current high bid.
public class BidPlacedConsumer : IConsumer<BidPlacedEvent>
{
    private readonly IAuctionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BidPlacedConsumer> _logger;

    public BidPlacedConsumer(
        IAuctionRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<BidPlacedConsumer> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<BidPlacedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation(
            "Consuming BidPlacedEvent {EventId} for auction {AuctionId}, Amount: {Amount}, Status: {Status}",
            message.Id, message.AuctionId, message.BidAmount, message.BidStatus);

        var auction = await _repository.GetByIdAsync(message.AuctionId, context.CancellationToken);
        if (auction == null)
        {
            _logger.LogWarning("Auction {AuctionId} not found for event {EventId}", message.AuctionId, message.Id);
            return;
        }

        if (!message.BidStatus.Contains("Accepted"))
        {
            _logger.LogDebug(
                "Bid {EventId} not accepted (Status: {Status}), skipping update",
                message.Id, message.BidStatus);
            return;
        }

        if (auction.CurrentHighBid.HasValue && message.BidAmount <= auction.CurrentHighBid.Value)
        {
            _logger.LogDebug(
                "Bid {EventId} amount {BidAmount} is not higher than current {CurrentBid}, skipping (idempotent)",
                message.Id, message.BidAmount, auction.CurrentHighBid.Value);
            return;
        }

        auction.UpdateHighBid(message.BidAmount, message.BidderId, message.Bidder);
        await _repository.UpdateAsync(auction, context.CancellationToken);
        await _unitOfWork.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation(
            "Updated auction {AuctionId} high bid to {Amount} by {Bidder}",
            message.AuctionId, message.BidAmount, message.Bidder);
    }
}

