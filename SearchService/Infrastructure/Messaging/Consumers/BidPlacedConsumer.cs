using Common.Messaging.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using SearchService.Application.Interfaces;

namespace SearchService.Infrastructure.Messaging.Consumers;

public class BidPlacedConsumer : IConsumer<BidPlacedEvent>
{
    private readonly ISearchItemRepository _repository;
    private readonly ILogger<BidPlacedConsumer> _logger;

    public BidPlacedConsumer(ISearchItemRepository repository, ILogger<BidPlacedConsumer> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<BidPlacedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Consuming BidPlacedEvent for auction {AuctionId}", message.AuctionId);

        var searchItem = await _repository.GetByIdAsync(message.AuctionId);
        
        if (searchItem == null)
        {
            _logger.LogWarning("Auction {AuctionId} not found in search index, skipping bid update", message.AuctionId);
            return;
        }

        if (searchItem.CurrentHighBid == null || message.BidStatus.Contains("Accepted")
            && message.BidAmount > searchItem.CurrentHighBid)
        {
            searchItem.CurrentHighBid = message.BidAmount;
            searchItem.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(searchItem);
        }
    }
}
