using Analytics.Api.Data;
using Analytics.Api.Entities;
using BidService.Contracts.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Analytics.Api.Consumers;

public class BidPlacedAnalyticsConsumer : IConsumer<BidPlacedEvent>
{
    private readonly AnalyticsDbContext _context;
    private readonly ILogger<BidPlacedAnalyticsConsumer> _logger;

    public BidPlacedAnalyticsConsumer(
        AnalyticsDbContext context,
        ILogger<BidPlacedAnalyticsConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<BidPlacedEvent> context)
    {
        var @event = context.Message;

        var exists = await _context.FactBids
            .AnyAsync(f => f.EventId == @event.Id, context.CancellationToken);

        if (exists)
        {
            _logger.LogWarning(
                "Duplicate BidPlaced event {EventId} skipped for Auction {AuctionId}",
                @event.Id, @event.AuctionId);
            return;
        }

        var fact = new FactBid
        {
            EventId = @event.Id,
            EventTime = @event.BidTime,
            IngestedAt = DateTimeOffset.UtcNow,

            AuctionId = @event.AuctionId,
            BidderId = @event.BidderId,
            DateKey = DateOnly.FromDateTime(@event.BidTime.UtcDateTime),

            BidAmount = @event.BidAmount,

            BidderUsername = @event.Bidder,

            BidStatus = @event.BidStatus,

            EventVersion = (short)@event.Version
        };

        _context.FactBids.Add(fact);
        await _context.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation(
            "Recorded BidPlaced fact: Auction={AuctionId}, Bidder={Bidder}, Amount={Amount}, Status={Status}",
            @event.AuctionId, @event.Bidder, @event.BidAmount, @event.BidStatus);
    }
}
