using Common.Messaging.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using SearchService.Application.Interfaces;

namespace SearchService.Infrastructure.Messaging.Consumers;

public class AuctionUpdatedConsumer : IConsumer<AuctionUpdatedEvent>
{
    private readonly ISearchItemRepository _repository;
    private readonly ILogger<AuctionUpdatedConsumer> _logger;

    public AuctionUpdatedConsumer(ISearchItemRepository repository, ILogger<AuctionUpdatedConsumer> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AuctionUpdatedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Consuming AuctionUpdatedEvent for auction {AuctionId}", message.Id);

        var searchItem = await _repository.GetBySourceIdAsync("auction", message.Id);
        if (searchItem == null)
        {
            _logger.LogWarning("Auction {AuctionId} not found in search index", message.Id);
            return;
        }

        if (message.Make != null) searchItem.Make = message.Make;
        if (message.Model != null) searchItem.Model = message.Model;
        if (message.Year.HasValue) searchItem.Year = message.Year.Value;
        if (message.Color != null) searchItem.Color = message.Color;
        if (message.Mileage.HasValue) searchItem.Mileage = message.Mileage.Value;

        searchItem.UpdatedAt = DateTime.UtcNow;
        if (searchItem.Metadata != null)
        {
            searchItem.Metadata.IndexedAt = DateTime.UtcNow;
        }

        await _repository.UpdateAsync(searchItem);
        _logger.LogInformation("Successfully updated auction {AuctionId} in search index", message.Id);
    }
}
