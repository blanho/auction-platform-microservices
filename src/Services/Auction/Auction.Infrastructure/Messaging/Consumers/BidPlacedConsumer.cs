using MassTransit;
using Microsoft.Extensions.Logging;

namespace Auctions.Infrastructure.Messaging.Consumers;

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
        _logger.LogInformation("Consuming BidPlacedEvent for auction {AuctionId}", message.AuctionId);

        var auction = await _repository.GetByIdAsync(message.AuctionId, context.CancellationToken);
        if (auction == null)
        {
            _logger.LogWarning("Auction {AuctionId} not found", message.AuctionId);
            return;
        }

        if (auction.CurrentHighBid == null || message.BidStatus.Contains("Accepted")
            && message.BidAmount > auction.CurrentHighBid)
        {
            auction.UpdateHighBid(message.BidAmount, message.BidderId, message.Bidder);
            await _repository.UpdateAsync(auction, context.CancellationToken);
            await _unitOfWork.SaveChangesAsync(context.CancellationToken);
        }
    }
}

