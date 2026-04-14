using AuctionService.Contracts.Events;
using Bidding.Application.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Bidding.Infrastructure.Messaging.Consumers;

public class AuctionStartedSnapshotConsumer : IConsumer<AuctionStartedEvent>
{
    private readonly IAuctionSnapshotRepository _snapshotRepository;
    private readonly ILogger<AuctionStartedSnapshotConsumer> _logger;

    public AuctionStartedSnapshotConsumer(
        IAuctionSnapshotRepository snapshotRepository,
        ILogger<AuctionStartedSnapshotConsumer> logger)
    {
        _snapshotRepository = snapshotRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AuctionStartedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation(
            "Consuming AuctionStartedEvent for {AuctionId}, updating local snapshot",
            message.AuctionId);

        var existing = await _snapshotRepository.GetAsync(message.AuctionId, context.CancellationToken);

        var snapshot = new AuctionSnapshot(
            AuctionId: message.AuctionId,
            Title: message.Title,
            SellerUsername: message.Seller,
            SellerId: existing?.SellerId ?? Guid.Empty,
            EndTime: message.EndTime,
            Status: "Live",
            ReservePrice: message.ReservePrice,
            CurrentHighBid: existing?.CurrentHighBid);

        await _snapshotRepository.UpsertAsync(snapshot, context.CancellationToken);
    }
}
