using BidService.Contracts.Events;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Abstractions.Providers;
using MassTransit;
using Search.Api.Services;

namespace Search.Api.Consumers;

public class BidPlacedConsumer : IConsumer<BidPlacedEvent>
{
    private readonly IAuctionIndexService _indexService;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger<BidPlacedConsumer> _logger;

    public BidPlacedConsumer(
        IAuctionIndexService indexService,
        IDateTimeProvider dateTime,
        ILogger<BidPlacedConsumer> logger)
    {
        _indexService = indexService;
        _dateTime = dateTime;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<BidPlacedEvent> context)
    {
        var message = context.Message;

        if (message.BidStatus != "Accepted")
        {
            _logger.LogDebug("Skipping non-accepted bid {BidId} with status {Status}",
                message.Id, message.BidStatus);
            return;
        }

        _logger.LogDebug("Processing BidPlacedEvent for auction {AuctionId}, amount {Amount}",
            message.AuctionId, message.BidAmount);

        var partialDocument = new Dictionary<string, object?>
        {
            ["currentPrice"] = message.BidAmount,
            ["lastSyncedAt"] = _dateTime.UtcNowOffset.ToString("o")
        };

        var success = await _indexService.PartialUpdateAsync(
            message.AuctionId,
            partialDocument,
            context.CancellationToken);

        if (!success)
        {
            _logger.LogWarning("Failed to update bid info for auction {AuctionId}", message.AuctionId);
        }
    }
}
