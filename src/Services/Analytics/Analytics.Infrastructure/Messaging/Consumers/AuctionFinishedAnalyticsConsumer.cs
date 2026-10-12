using Microsoft.EntityFrameworkCore;
using MassTransit;
using Microsoft.Extensions.Logging;
using Analytics.Domain.Constants;
using Analytics.Infrastructure.Persistence;
using Analytics.Domain.Entities;
using AuctionService.Contracts.Events;

namespace Analytics.Infrastructure.Messaging.Consumers;

public class AuctionFinishedAnalyticsConsumer : IConsumer<AuctionFinishedEvent>
{
    private readonly AnalyticsDbContext _context;
    private readonly ILogger<AuctionFinishedAnalyticsConsumer> _logger;

    public AuctionFinishedAnalyticsConsumer(
        AnalyticsDbContext context,
        ILogger<AuctionFinishedAnalyticsConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AuctionFinishedEvent> context)
    {
        var @event = context.Message;

        var exists = await _context.FactAuctions
            .AnyAsync(f => f.AuctionId == @event.AuctionId && f.EventType == AnalyticsEventTypes.Finished,
                context.CancellationToken);

        if (exists)
        {
            _logger.LogWarning(
                "Duplicate AuctionFinished event skipped for Auction {AuctionId}",
                @event.AuctionId);
            return;
        }

        var now = DateTimeOffset.UtcNow;

        var fact = new FactAuction
        {
            EventId = Guid.NewGuid(),
            AuctionId = @event.AuctionId,
            EventTime = now,
            IngestedAt = now,
            SellerId = @event.SellerId,
            WinnerId = @event.WinnerId,
            DateKey = DateOnly.FromDateTime(now.UtcDateTime),

            Title = string.Empty,
            SellerUsername = @event.SellerUsername,
            WinnerUsername = @event.WinnerUsername,

            FinalPrice = @event.SoldAmount,

            Status = @event.ItemSold ? AnalyticsAuctionStatuses.Sold : AnalyticsAuctionStatuses.Ended,
            Sold = @event.ItemSold,
            EndedAt = now,

            EventType = AnalyticsEventTypes.Finished,
            EventVersion = (short)@event.Version
        };

        _context.FactAuctions.Add(fact);
        await _context.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation(
            "Recorded AuctionFinished fact for {AuctionId}, Sold: {Sold}, Amount: {Amount}",
            @event.AuctionId, @event.ItemSold, @event.SoldAmount);
    }
}
