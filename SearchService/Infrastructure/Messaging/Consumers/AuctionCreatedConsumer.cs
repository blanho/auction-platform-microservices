using Common.Messaging.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using SearchService.Application.Interfaces;
using SearchService.Domain.Entities;

namespace SearchService.Infrastructure.Messaging.Consumers;

public class AuctionCreatedConsumer : IConsumer<AuctionCreatedEvent>
{
    private readonly ISearchItemRepository _repository;
    private readonly ILogger<AuctionCreatedConsumer> _logger;

    public AuctionCreatedConsumer(ISearchItemRepository repository, ILogger<AuctionCreatedConsumer> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AuctionCreatedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Consuming AuctionCreatedEvent for auction {AuctionId}", message.Id);

        var searchItem = new SearchItem
        {
            Id = message.Id,
            Title = message.Title,
            Description = message.Description,
            Make = message.Make,
            Model = message.Model,
            Year = message.Year,
            Color = message.Color,
            Mileage = message.Mileage,
            ImageUrl = message.ImageUrl,
            Status = message.Status,
            CreatedAt = message.CreatedAt,
            UpdatedAt = message.UpdatedAt,
            AuctionEnd = message.AuctionEnd,
            Seller = message.Seller,
            Winner = message.Winner,
            SoldAmount = message.SoldAmount,
            CurrentHighBid = message.CurrentHighBid,
            ReservePrice = message.ReservePrice,
            Source = "auction",
            SourceId = message.Id,
            Metadata = new SearchMetadata
            {
                IndexedAt = DateTime.UtcNow,
                Source = "auction"
            }
        };

        await _repository.CreateAsync(searchItem);
        _logger.LogInformation("Successfully indexed auction {AuctionId} to search", message.Id);
    }
}
