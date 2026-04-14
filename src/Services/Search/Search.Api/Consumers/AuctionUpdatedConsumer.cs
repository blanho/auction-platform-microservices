using AuctionService.Contracts.Events;
using BuildingBlocks.Application.Abstractions.Providers;
using MassTransit;
using Search.Api.Constants;
using Search.Api.Services;

namespace Search.Api.Consumers;

public class AuctionUpdatedConsumer : IConsumer<AuctionUpdatedEvent>
{
    private readonly IAuctionIndexService _indexService;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger<AuctionUpdatedConsumer> _logger;

    public AuctionUpdatedConsumer(
        IAuctionIndexService indexService,
        IDateTimeProvider dateTime,
        ILogger<AuctionUpdatedConsumer> logger)
    {
        _indexService = indexService;
        _dateTime = dateTime;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AuctionUpdatedEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation("Processing AuctionUpdatedEvent for auction {AuctionId}", message.Id);

        var partialDocument = new Dictionary<string, object?>
        {
            [ElasticsearchFields.UpdatedAt] = _dateTime.UtcNowOffset,
            [ElasticsearchFields.LastSyncedAt] = _dateTime.UtcNowOffset
        };

        if (message.Title != null)
            partialDocument[ElasticsearchFields.Title] = message.Title;

        if (message.Description != null)
            partialDocument[ElasticsearchFields.Description] = message.Description;

        if (message.Condition != null)
            partialDocument[ElasticsearchFields.Condition] = message.Condition;

        var result = await _indexService.PartialUpdateAsync(
            message.Id,
            partialDocument,
            context.CancellationToken);

        if (result.IsFailure)
        {
            _logger.LogWarning("Failed to update auction {AuctionId}: {Error}", message.Id, result.Error);
        }
        else
        {
            _logger.LogInformation("Successfully updated auction {AuctionId}", message.Id);
        }
    }
}
