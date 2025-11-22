using Common.Messaging.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using SearchService.Application.Interfaces;

namespace SearchService.Infrastructure.Messaging.Consumers;

public class AuctionDeletedConsumer : IConsumer<AuctionDeletedEvent>
{
    private readonly ISearchItemRepository _repository;
    private readonly ILogger<AuctionDeletedConsumer> _logger;

    public AuctionDeletedConsumer(ISearchItemRepository repository, ILogger<AuctionDeletedConsumer> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AuctionDeletedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Consuming AuctionDeletedEvent for auction {AuctionId}", message.Id);
        await _repository.DeleteAsync(message.Id);
        _logger.LogInformation("Successfully removed auction {AuctionId} from search index", message.Id);
    }
}
