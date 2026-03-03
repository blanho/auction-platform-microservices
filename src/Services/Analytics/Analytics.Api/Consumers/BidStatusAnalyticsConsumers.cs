using Analytics.Api.Data;
using Analytics.Api.Entities;
using BidService.Contracts.Events;
using MassTransit;

namespace Analytics.Api.Consumers;

public class BidBelowReserveAnalyticsConsumer : IConsumer<BidAcceptedBelowReserveEvent>
{
    private readonly AnalyticsDbContext _context;
    private readonly ILogger<BidBelowReserveAnalyticsConsumer> _logger;

    public BidBelowReserveAnalyticsConsumer(
        AnalyticsDbContext context,
        ILogger<BidBelowReserveAnalyticsConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<BidAcceptedBelowReserveEvent> context)
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
            BidStatus = "AcceptedBelowReserve",
            EventVersion = (short)@event.Version
        };

        _context.FactBids.Add(fact);
        await _context.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation(
            "Recorded BidBelowReserve fact: Auction={AuctionId}, Bidder={Bidder}, Amount={Amount}",
            @event.AuctionId, @event.BidderUsername, @event.Amount);
    }
}

public class BidTooLowAnalyticsConsumer : IConsumer<BidMarkedTooLowEvent>
{
    private readonly AnalyticsDbContext _context;
    private readonly ILogger<BidTooLowAnalyticsConsumer> _logger;

    public BidTooLowAnalyticsConsumer(
        AnalyticsDbContext context,
        ILogger<BidTooLowAnalyticsConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<BidMarkedTooLowEvent> context)
    {
        var @event = context.Message;

        var fact = new FactBid
        {
            EventId = @event.BidId,
            EventTime = @event.MarkedAt,
            IngestedAt = DateTimeOffset.UtcNow,
            AuctionId = @event.AuctionId,
            BidderId = @event.BidderId,
            DateKey = DateOnly.FromDateTime(@event.MarkedAt.UtcDateTime),
            BidAmount = @event.Amount,
            BidStatus = "TooLow",
            EventVersion = (short)@event.Version
        };

        _context.FactBids.Add(fact);
        await _context.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation(
            "Recorded BidTooLow fact: Auction={AuctionId}, Bidder={BidderId}, Amount={Amount}",
            @event.AuctionId, @event.BidderId, @event.Amount);
    }
}
