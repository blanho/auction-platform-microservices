using Analytics.Api.Data;
using Analytics.Api.Entities;
using BidService.Contracts.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Analytics.Api.Consumers;

public class HighestBidUpdatedAnalyticsConsumer : IConsumer<HighestBidUpdatedEvent>
{
    private readonly AnalyticsDbContext _context;
    private readonly ILogger<HighestBidUpdatedAnalyticsConsumer> _logger;

    public HighestBidUpdatedAnalyticsConsumer(
        AnalyticsDbContext context,
        ILogger<HighestBidUpdatedAnalyticsConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<HighestBidUpdatedEvent> context)
    {
        var @event = context.Message;

        var exists = await _context.FactBids
            .AnyAsync(f => f.EventId == @event.Id, context.CancellationToken);

        if (exists)
        {
            _logger.LogWarning(
                "Duplicate HighestBidUpdated event {EventId} skipped for Auction {AuctionId}",
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
            BidAmount = @event.NewHighestAmount,
            BidderUsername = @event.BidderUsername,
            BidStatus = @event.BidStatus,
            EventVersion = (short)@event.Version
        };

        _context.FactBids.Add(fact);
        await _context.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation(
            "Recorded HighestBidUpdated fact: Auction={AuctionId}, Bidder={Bidder}, Amount={Amount}, Status={Status}",
            @event.AuctionId, @event.BidderUsername, @event.NewHighestAmount, @event.BidStatus);
    }
}
