using Analytics.Api.Data;
using Analytics.Api.Entities;
using BidService.Contracts.Events;
using MassTransit;

namespace Analytics.Api.Consumers;

public class BidRetractedAnalyticsConsumer : IConsumer<BidRetractedEvent>
{
    private readonly AnalyticsDbContext _context;
    private readonly ILogger<BidRetractedAnalyticsConsumer> _logger;

    public BidRetractedAnalyticsConsumer(
        AnalyticsDbContext context,
        ILogger<BidRetractedAnalyticsConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<BidRetractedEvent> context)
    {
        var @event = context.Message;

        var fact = new FactBid
        {
            EventId = @event.BidId,
            EventTime = @event.RetractedAt,
            IngestedAt = DateTimeOffset.UtcNow,
            AuctionId = @event.AuctionId,
            BidderId = @event.BidderId,
            DateKey = DateOnly.FromDateTime(@event.RetractedAt.UtcDateTime),
            BidAmount = @event.BidAmount,
            BidderUsername = @event.Bidder,
            BidStatus = "Retracted",
            EventVersion = (short)@event.Version
        };

        _context.FactBids.Add(fact);
        await _context.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation(
            "Recorded BidRetracted fact: Auction={AuctionId}, Bidder={Bidder}, Amount={Amount}, Reason={Reason}",
            @event.AuctionId, @event.Bidder, @event.BidAmount, @event.Reason);
    }
}
