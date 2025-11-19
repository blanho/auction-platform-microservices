using AuctionService.Contracts.Events;
using BuildingBlocks.Infrastructure.Messaging;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using Search.Api.Services;

namespace Search.Api.Consumers;

public class AuctionDeletedConsumer : IConsumer<AuctionDeletedEvent>
{
    private readonly IAuctionIndexService _indexService;
    private readonly ILogger<AuctionDeletedConsumer> _logger;

    public AuctionDeletedConsumer(
        IAuctionIndexService indexService,
        ILogger<AuctionDeletedConsumer> logger)
    {
        _indexService = indexService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AuctionDeletedEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation("Processing AuctionDeletedEvent for auction {AuctionId}", message.Id);

        var success = await _indexService.DeleteAsync(message.Id, context.CancellationToken);

        if (!success)
        {
            _logger.LogWarning("Failed to delete auction {AuctionId} - document may not exist", message.Id);
        }
        else
        {
            _logger.LogInformation("Successfully deleted auction {AuctionId} from index", message.Id);
        }
    }
}
