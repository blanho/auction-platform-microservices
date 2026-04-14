using AuctionService.Contracts.Events;
using Bidding.Application.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Bidding.Infrastructure.Messaging.Consumers;

public class AuctionFinishedSnapshotConsumer : IConsumer<AuctionFinishedEvent>
{
    private readonly IAuctionSnapshotRepository _snapshotRepository;
    private readonly ILogger<AuctionFinishedSnapshotConsumer> _logger;

    public AuctionFinishedSnapshotConsumer(
        IAuctionSnapshotRepository snapshotRepository,
        ILogger<AuctionFinishedSnapshotConsumer> logger)
    {
        _snapshotRepository = snapshotRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AuctionFinishedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation(
            "Consuming AuctionFinishedEvent for {AuctionId}, marking snapshot as Finished",
            message.AuctionId);

        var existing = await _snapshotRepository.GetAsync(message.AuctionId, context.CancellationToken);
        if (existing == null)
        {
            _logger.LogWarning(
                "Snapshot not found for finished auction {AuctionId}, skipping update",
                message.AuctionId);
            return;
        }

        var updated = existing with { Status = "Finished" };
        await _snapshotRepository.UpsertAsync(updated, context.CancellationToken);
    }
}
