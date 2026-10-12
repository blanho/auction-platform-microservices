using Microsoft.EntityFrameworkCore;
using MassTransit;
using Microsoft.Extensions.Logging;
using Analytics.Domain.Constants;
using Analytics.Infrastructure.Persistence;
using Analytics.Domain.Entities;
using AuctionService.Contracts.Events;

namespace Analytics.Infrastructure.Messaging.Consumers;

public class AuctionStartedAnalyticsConsumer : IConsumer<AuctionStartedEvent>
{
    private readonly AnalyticsDbContext _context;
    private readonly ILogger<AuctionStartedAnalyticsConsumer> _logger;

    public AuctionStartedAnalyticsConsumer(
        AnalyticsDbContext context,
        ILogger<AuctionStartedAnalyticsConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AuctionStartedEvent> context)
    {
        var @event = context.Message;

        var exists = await _context.FactAuctions
            .AnyAsync(f => f.AuctionId == @event.AuctionId && f.EventType == AnalyticsEventTypes.Started,
                context.CancellationToken);

        if (exists)
        {
            _logger.LogWarning(
                "Duplicate AuctionStarted event skipped for Auction {AuctionId}", @event.AuctionId);
            return;
        }

        var now = DateTimeOffset.UtcNow;

        var fact = new FactAuction
        {
            EventId = Guid.NewGuid(),
            AuctionId = @event.AuctionId,
            EventTime = @event.StartTime,
            IngestedAt = now,
            DateKey = DateOnly.FromDateTime(@event.StartTime.UtcDateTime),

            Title = @event.Title,
            SellerUsername = @event.Seller,

            ReservePrice = @event.ReservePrice,
            StartedAt = @event.StartTime,

            Status = AnalyticsAuctionStatuses.Live,
            HadReserve = @event.ReservePrice > 0,

            EventType = AnalyticsEventTypes.Started,
            EventVersion = (short)@event.Version
        };

        _context.FactAuctions.Add(fact);
        await _context.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation(
            "Recorded AuctionStarted fact for {AuctionId}, Seller: {Seller}",
            @event.AuctionId, @event.Seller);
    }
}
