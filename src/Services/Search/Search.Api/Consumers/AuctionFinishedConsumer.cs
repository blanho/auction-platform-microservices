using AuctionService.Contracts.Events;
using BuildingBlocks.Application.Abstractions.Providers;
using BuildingBlocks.Infrastructure.Messaging;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
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
            ["status"] = message.ItemSold ? "Sold" : "Finished",
            ["lastSyncedAt"] = _dateTime.UtcNowOffset.ToString("o")
        };

        if (message.ItemSold)
        {
            partialDocument["winningBidderId"] = message.WinnerId?.ToString();
            partialDocument["winningBidderUsername"] = message.WinnerUsername;
            partialDocument["winningBidAmount"] = message.SoldAmount;
            partialDocument["currentPrice"] = message.SoldAmount;
        }

        var success = await _indexService.PartialUpdateAsync(
            message.AuctionId,
            partialDocument,
            context.CancellationToken);

        if (!success)
        {
            _logger.LogWarning("Failed to update finished auction {AuctionId}", message.AuctionId);
        }
        else
        {
            _logger.LogInformation("Successfully updated finished auction {AuctionId}, Sold: {ItemSold}",
                message.AuctionId, message.ItemSold);
        }
    }
}
