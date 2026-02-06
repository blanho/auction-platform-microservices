using BidService.Contracts.Events;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Abstractions.Providers;
using MassTransit;
using Search.Api.Services;

namespace Search.Api.Consumers;

public class HighestBidUpdatedConsumer : IConsumer<HighestBidUpdatedEvent>
{
    private readonly IAuctionIndexService _indexService;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger<HighestBidUpdatedConsumer> _logger;

    public HighestBidUpdatedConsumer(
        IAuctionIndexService indexService,
        IDateTimeProvider dateTime,
        ILogger<HighestBidUpdatedConsumer> logger)
    {
        _indexService = indexService;
        _dateTime = dateTime;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<HighestBidUpdatedEvent> context)
    {
        var message = context.Message;

        if (message.BidStatus != "Accepted")
        {
            _logger.LogDebug("Skipping non-accepted highest bid update {BidId} with status {Status}",
                message.Id, message.BidStatus);
            return;
        }

        _logger.LogDebug("Processing HighestBidUpdatedEvent for auction {AuctionId}, amount {Amount}",
            message.AuctionId, message.NewHighestAmount);

        var partialDocument = new Dictionary<string, object?>
        {
            ["currentPrice"] = message.NewHighestAmount,
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
