using AuctionService.Contracts.Events;
using Bidding.Application.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Bidding.Infrastructure.Messaging.Consumers;

public class AuctionCreatedSnapshotConsumer : IConsumer<AuctionCreatedEvent>
{
    private readonly IAuctionSnapshotRepository _snapshotRepository;
    private readonly ILogger<AuctionCreatedSnapshotConsumer> _logger;

    public AuctionCreatedSnapshotConsumer(
        IAuctionSnapshotRepository snapshotRepository,
        ILogger<AuctionCreatedSnapshotConsumer> logger)
    {
        _snapshotRepository = snapshotRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AuctionCreatedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation(
            "Consuming AuctionCreatedEvent for {AuctionId}, updating local snapshot",
            message.Id);

        var snapshot = new AuctionSnapshot(
            AuctionId: message.Id,
            Title: message.Title,
            SellerUsername: message.SellerUsername,
            SellerId: message.SellerId,
            EndTime: message.AuctionEnd ?? DateTimeOffset.MaxValue,
            Status: message.Status,
            ReservePrice: message.ReservePrice,
            CurrentHighBid: message.CurrentHighBid);

        await _snapshotRepository.UpsertAsync(snapshot, context.CancellationToken);
    }
}
