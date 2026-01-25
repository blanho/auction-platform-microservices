using Analytics.Api.Data;
using Analytics.Api.Entities;
using BidService.Contracts.Events;
using MassTransit;

namespace Analytics.Api.Consumers;

public class BidAcceptedAnalyticsConsumer : IConsumer<BidAcceptedEvent>
{
    private readonly AnalyticsDbContext _context;
    private readonly ILogger<BidAcceptedAnalyticsConsumer> _logger;

    public BidAcceptedAnalyticsConsumer(
        AnalyticsDbContext context,
        ILogger<BidAcceptedAnalyticsConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<BidAcceptedEvent> context)
    {
        var @event = context.Message;

        var fact = new FactBid
        {
            EventId = @event.BidId,
            EventTime = @event.AcceptedAt,
            IngestedAt = DateTimeOffset.UtcNow,
            AuctionId = @event.AuctionId,
            BidderId = @event.BidderId,
            DateKey = DateOnly.FromDateTime(@event.AcceptedAt.UtcDateTime),
            BidAmount = @event.Amount,
            BidderUsername = @event.BidderUsername,
            BidStatus = "Accepted",
            EventVersion = (short)@event.Version
        };

        _context.FactBids.Add(fact);
        await _context.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation(
            "Recorded BidAccepted fact: Auction={AuctionId}, Bidder={Bidder}, Amount={Amount}",
            @event.AuctionId, @event.BidderUsername, @event.Amount);
    }
}
