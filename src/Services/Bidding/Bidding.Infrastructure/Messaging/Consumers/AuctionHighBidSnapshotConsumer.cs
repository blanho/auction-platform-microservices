using AuctionService.Contracts.Events;
using Bidding.Application.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Bidding.Infrastructure.Messaging.Consumers;

public class AuctionHighBidSnapshotConsumer : IConsumer<AuctionHighBidUpdatedEvent>
{
    private readonly IAuctionSnapshotRepository _snapshotRepository;
    private readonly ILogger<AuctionHighBidSnapshotConsumer> _logger;

    public AuctionHighBidSnapshotConsumer(
        IAuctionSnapshotRepository snapshotRepository,
        ILogger<AuctionHighBidSnapshotConsumer> logger)
    {
        _snapshotRepository = snapshotRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AuctionHighBidUpdatedEvent> context)
    {
        var message = context.Message;
        _logger.LogDebug(
            "Consuming AuctionHighBidUpdatedEvent for {AuctionId}, amount: {Amount}",
            message.AuctionId, message.BidAmount);

        var existing = await _snapshotRepository.GetAsync(message.AuctionId, context.CancellationToken);
        if (existing == null)
        {
            _logger.LogWarning("Snapshot not found for {AuctionId}, skipping update", message.AuctionId);
            return;
        }

        var updated = existing with { CurrentHighBid = message.BidAmount };
        await _snapshotRepository.UpsertAsync(updated, context.CancellationToken);
    }
}
