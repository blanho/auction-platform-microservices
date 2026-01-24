using BidService.Contracts.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Auctions.Infrastructure.Messaging.Consumers;

public class BidRetractedConsumer : IConsumer<BidRetractedEvent>
{
    private readonly IAuctionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BidRetractedConsumer> _logger;

    public BidRetractedConsumer(
        IAuctionRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<BidRetractedConsumer> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<BidRetractedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation(
            "Consuming BidRetracted event for auction {AuctionId}",
            message.AuctionId);

        var auction = await _repository.GetByIdAsync(message.AuctionId, context.CancellationToken);
        if (auction == null)
        {
            _logger.LogWarning("Auction {AuctionId} not found", message.AuctionId);
            return;
        }

        if (message.NewHighestBidId.HasValue && message.NewHighestAmount.HasValue && message.NewHighestBidderId.HasValue)
        {
            auction.UpdateHighBid(
                message.NewHighestAmount.Value,
                message.NewHighestBidderId.Value,
                string.Empty);
            
            _logger.LogInformation(
                "Updated auction {AuctionId} with new high bid {Amount} after retraction",
                message.AuctionId, message.NewHighestAmount.Value);
        }
        else
        {
            auction.UpdateHighBid(0, null, null);
            
            _logger.LogInformation(
                "Cleared high bid for auction {AuctionId} after retraction (no remaining bids)",
                message.AuctionId);
        }

        await _repository.UpdateAsync(auction, context.CancellationToken);
        await _unitOfWork.SaveChangesAsync(context.CancellationToken);
    }
}
