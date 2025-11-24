using Common.Messaging.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using SearchService.Application.Interfaces;
using Common.Domain.Enums;

namespace SearchService.Infrastructure.Messaging.Consumers;

public class AuctionFinishedConsumer : IConsumer<AuctionFinishedEvent>
{
    private readonly ISearchItemRepository _repository;
    private readonly ILogger<AuctionFinishedConsumer> _logger;

    public AuctionFinishedConsumer(ISearchItemRepository repository, ILogger<AuctionFinishedConsumer> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AuctionFinishedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Consuming AuctionFinishedEvent for auction {AuctionId}, ItemSold: {ItemSold}", 
            message.AuctionId, message.ItemSold);

        var searchItem = await _repository.GetByIdAsync(message.AuctionId);
        
        if (searchItem == null)
        {
            _logger.LogWarning("Auction {AuctionId} not found in search index, skipping finish update", message.AuctionId);
            return;
        }

        // Update auction completion details
        if (message.ItemSold)
        {
            searchItem.Winner = message.Winner;
            searchItem.SoldAmount = message.SoldAmount;
        }
        
        searchItem.Status = message.SoldAmount > searchItem.ReservePrice
            ? Status.Finished.ToString()
            : Status.ReservedNotMet.ToString();

        searchItem.UpdatedAt = DateTime.UtcNow;

        _logger.LogInformation("Auction {AuctionId} finished - sold to {Winner} for {Amount}",
            message.AuctionId, message.Winner, message.SoldAmount);
        
        await _repository.UpdateAsync(searchItem);
        _logger.LogInformation("Successfully updated auction {AuctionId} status to {Status} in search index",
            message.AuctionId, searchItem.Status);
    }
}
