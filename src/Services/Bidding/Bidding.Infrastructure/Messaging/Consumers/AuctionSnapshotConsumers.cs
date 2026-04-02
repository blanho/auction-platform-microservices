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
