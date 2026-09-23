using MassTransit;
using Microsoft.Extensions.Logging;
using Search.Application.Interfaces;

using BidService.Contracts.Events;
using BidService.Contracts.Constants;

namespace Search.Infrastructure.Consumers;

public class HighestBidUpdatedConsumer : IConsumer<HighestBidUpdatedEvent>
{
    private readonly IAuctionIndexService _indexService;
    private readonly ILogger<HighestBidUpdatedConsumer> _logger;

    public HighestBidUpdatedConsumer(
        IAuctionIndexService indexService,
        ILogger<HighestBidUpdatedConsumer> logger)
    {
        _indexService = indexService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<HighestBidUpdatedEvent> context)
    {
        var message = context.Message;

        if (message.BidStatus != BidEventStatusNames.Accepted)
        {
            _logger.LogDebug("Skipping non-accepted highest bid update {BidId} with status {Status}",
                message.Id, message.BidStatus);
            return;
        }

        _logger.LogDebug("Processing HighestBidUpdatedEvent for auction {AuctionId}, amount {Amount}",
            message.AuctionId, message.NewHighestAmount);

        var result = await _indexService.ApplyBidStateAsync(
            message.AuctionId,
            message.NewHighestAmount,
            message.BidTime,
            false,
            context.CancellationToken);

        if (result.IsFailure)
        {
            _logger.LogWarning("Failed to update bid info for auction {AuctionId}: {Error}",
                message.AuctionId, result.Error);
            throw new InvalidOperationException($"Search index operation failed: {result.Error}");
        }
    }
}
