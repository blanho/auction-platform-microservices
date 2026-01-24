using BidService.Contracts.Events;
using BuildingBlocks.Application.Abstractions.Providers;
using MassTransit;
using Search.Api.Services;

namespace Search.Api.Consumers;

public class BidRetractedConsumer : IConsumer<BidRetractedEvent>
{
    private readonly IAuctionIndexService _indexService;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger<BidRetractedConsumer> _logger;

    public BidRetractedConsumer(
        IAuctionIndexService indexService,
        IDateTimeProvider dateTime,
        ILogger<BidRetractedConsumer> logger)
    {
        _indexService = indexService;
        _dateTime = dateTime;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<BidRetractedEvent> context)
    {
        var message = context.Message;

        _logger.LogDebug(
            "Processing BidRetracted event for auction {AuctionId}",
            message.AuctionId);

        var partialDocument = new Dictionary<string, object?>
        {
            ["currentPrice"] = message.NewHighestAmount ?? 0m,
            ["lastSyncedAt"] = _dateTime.UtcNowOffset.ToString("o")
        };

        var success = await _indexService.PartialUpdateAsync(
            message.AuctionId,
            partialDocument,
            context.CancellationToken);

        if (!success)
        {
            _logger.LogWarning(
                "Failed to update search index after bid retraction for auction {AuctionId}",
                message.AuctionId);
        }
    }
}
