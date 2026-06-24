using Microsoft.EntityFrameworkCore;
using MassTransit;
using Microsoft.Extensions.Logging;
using Analytics.Infrastructure.Persistence;
using BidService.Contracts.Events;

namespace Analytics.Infrastructure.Messaging.Consumers;

public class OutbidAnalyticsConsumer : IConsumer<OutbidEvent>
{
    private readonly AnalyticsDbContext _context;
    private readonly ILogger<OutbidAnalyticsConsumer> _logger;

    public OutbidAnalyticsConsumer(
        AnalyticsDbContext context,
        ILogger<OutbidAnalyticsConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OutbidEvent> context)
    {
        var @event = context.Message;

        var auction = await _context.FactAuctions
            .FirstOrDefaultAsync(a => a.AuctionId == @event.AuctionId, context.CancellationToken);

        if (auction != null)
        {
            auction.TotalBids += 1;
        }

        await _context.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation(
            "Recorded Outbid event: Auction={AuctionId}, OutbidUser={OutbidUser}, NewHighBidder={NewBidder}, NewAmount={Amount}",
            @event.AuctionId, @event.OutbidBidderUsername, @event.NewHighBidderUsername, @event.NewHighBidAmount);
    }
}
