using AuctionService.Contracts.Events;
using Bidding.Application.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Bidding.Infrastructure.Messaging.Consumers;

public class AuctionDeletedSnapshotConsumer : IConsumer<AuctionDeletedEvent>
{
    private readonly IAuctionSnapshotRepository _snapshotRepository;
    private readonly ILogger<AuctionDeletedSnapshotConsumer> _logger;

    public AuctionDeletedSnapshotConsumer(
        IAuctionSnapshotRepository snapshotRepository,
        ILogger<AuctionDeletedSnapshotConsumer> logger)
    {
        _snapshotRepository = snapshotRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AuctionDeletedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation(
            "Consuming AuctionDeletedEvent for {AuctionId}, removing local snapshot",
            message.Id);

        await _snapshotRepository.DeleteAsync(message.Id, context.CancellationToken);
    }
}
