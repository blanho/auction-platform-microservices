using MassTransit;
using Microsoft.Extensions.Logging;
using Search.Application.Interfaces;

using BidService.Contracts.Events;

namespace Search.Infrastructure.Consumers;

public class BidRetractedConsumer : IConsumer<BidRetractedEvent>
{
    private readonly IAuctionIndexService _indexService;
    private readonly ILogger<BidRetractedConsumer> _logger;

    public BidRetractedConsumer(
        IAuctionIndexService indexService,
        ILogger<BidRetractedConsumer> logger)
    {
        _indexService = indexService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<BidRetractedEvent> context)
    {
        var message = context.Message;
        if (!message.WasHighestBid)
            return;

        _logger.LogDebug(
            "Processing BidRetracted event for auction {AuctionId}",
            message.AuctionId);

        var result = await _indexService.ApplyBidStateAsync(
            message.AuctionId,
            message.NewHighestAmount,
            message.RetractedAt,
            true,
            context.CancellationToken);

        if (result.IsFailure)
        {
            _logger.LogWarning(
                "Failed to update search index after bid retraction for auction {AuctionId}: {Error}",
                message.AuctionId,
                result.Error);
            throw new InvalidOperationException($"Search index operation failed: {result.Error}");
        }
    }
}
