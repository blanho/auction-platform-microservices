using Analytics.Api.Data;
using Analytics.Api.Entities;
using BidService.Contracts.Events;
using MassTransit;

namespace Analytics.Api.Consumers;

public class BidRejectedAnalyticsConsumer : IConsumer<BidRejectedEvent>
{
    private readonly AnalyticsDbContext _context;
    private readonly ILogger<BidRejectedAnalyticsConsumer> _logger;

    public BidRejectedAnalyticsConsumer(
        AnalyticsDbContext context,
        ILogger<BidRejectedAnalyticsConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<BidRejectedEvent> context)
    {
        var @event = context.Message;

        var fact = new FactBid
        {
            EventId = @event.BidId,
            EventTime = @event.RejectedAt,
            IngestedAt = DateTimeOffset.UtcNow,
            AuctionId = @event.AuctionId,
            BidderId = @event.BidderId,
            DateKey = DateOnly.FromDateTime(@event.RejectedAt.UtcDateTime),
            BidAmount = @event.Amount,
            BidderUsername = @event.BidderUsername,
            BidStatus = "Rejected",
            EventVersion = (short)@event.Version
        };

        _context.FactBids.Add(fact);
        await _context.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation(
            "Recorded BidRejected fact: Auction={AuctionId}, Bidder={Bidder}, Amount={Amount}, Reason={Reason}",
            @event.AuctionId, @event.BidderUsername, @event.Amount, @event.Reason);
    }
}
