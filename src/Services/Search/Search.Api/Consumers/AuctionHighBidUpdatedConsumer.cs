using AuctionService.Contracts.Events;
using BuildingBlocks.Application.Abstractions.Providers;
using MassTransit;
using Search.Api.Constants;
using Search.Api.Services;

namespace Search.Api.Consumers;

public class AuctionHighBidUpdatedConsumer : IConsumer<AuctionHighBidUpdatedEvent>
{
    private readonly IAuctionIndexService _indexService;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger<AuctionHighBidUpdatedConsumer> _logger;

    public AuctionHighBidUpdatedConsumer(
        IAuctionIndexService indexService,
        IDateTimeProvider dateTime,
        ILogger<AuctionHighBidUpdatedConsumer> logger)
    {
        _indexService = indexService;
        _dateTime = dateTime;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AuctionHighBidUpdatedEvent> context)
    {
        var message = context.Message;

        _logger.LogDebug(
            "Processing AuctionHighBidUpdatedEvent for auction {AuctionId}, amount {Amount}",
            message.AuctionId, message.BidAmount);

        var partialDocument = new Dictionary<string, object?>
        {
            [ElasticsearchFields.CurrentPrice] = message.BidAmount,
            [ElasticsearchFields.LastSyncedAt] = _dateTime.UtcNowOffset.ToString(DateTimeFormats.Iso8601)
        };

        var result = await _indexService.PartialUpdateAsync(
            message.AuctionId,
            partialDocument,
            context.CancellationToken);

        if (result.IsFailure)
        {
            _logger.LogWarning(
                "Failed to update high bid for auction {AuctionId}: {Error}",
                message.AuctionId, result.Error);
        }
    }
}
