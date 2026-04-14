using AuctionService.Contracts.Events;
using BuildingBlocks.Application.Abstractions.Providers;
using MassTransit;
using Search.Api.Constants;
using Search.Api.Services;

namespace Search.Api.Consumers;

public class AuctionFinishedConsumer : IConsumer<AuctionFinishedEvent>
{
    private readonly IAuctionIndexService _indexService;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger<AuctionFinishedConsumer> _logger;

    public AuctionFinishedConsumer(
        IAuctionIndexService indexService,
        IDateTimeProvider dateTime,
        ILogger<AuctionFinishedConsumer> logger)
    {
        _indexService = indexService;
        _dateTime = dateTime;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AuctionFinishedEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation("Processing AuctionFinishedEvent for auction {AuctionId}", message.AuctionId);

        var partialDocument = new Dictionary<string, object?>
        {
            [ElasticsearchFields.Status] = message.ItemSold ? AuctionStatuses.Sold : AuctionStatuses.Finished,
            [ElasticsearchFields.LastSyncedAt] = _dateTime.UtcNowOffset.ToString(DateTimeFormats.Iso8601)
        };

        if (message.ItemSold)
        {
            partialDocument[ElasticsearchFields.WinningBidderId] = message.WinnerId?.ToString();
            partialDocument[ElasticsearchFields.WinningBidderUsername] = message.WinnerUsername;
            partialDocument[ElasticsearchFields.WinningBidAmount] = message.SoldAmount;
            partialDocument[ElasticsearchFields.CurrentPrice] = message.SoldAmount;
        }

        var result = await _indexService.PartialUpdateAsync(
            message.AuctionId,
            partialDocument,
            context.CancellationToken);

        if (result.IsFailure)
        {
            _logger.LogWarning("Failed to update finished auction {AuctionId}: {Error}",
                message.AuctionId, result.Error);
        }
        else
        {
            _logger.LogInformation("Successfully updated finished auction {AuctionId}, Sold: {ItemSold}",
                message.AuctionId, message.ItemSold);
        }
    }
}
