using Microsoft.EntityFrameworkCore;
using MassTransit;
using Microsoft.Extensions.Logging;
using Analytics.Domain.Constants;
using Analytics.Infrastructure.Persistence;
using Analytics.Domain.Entities;
using AuctionService.Contracts.Events;

namespace Analytics.Infrastructure.Messaging.Consumers;

public class BuyNowExecutedAnalyticsConsumer : IConsumer<BuyNowExecutedEvent>
{
    private readonly AnalyticsDbContext _context;
    private readonly ILogger<BuyNowExecutedAnalyticsConsumer> _logger;

    public BuyNowExecutedAnalyticsConsumer(
        AnalyticsDbContext context,
        ILogger<BuyNowExecutedAnalyticsConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<BuyNowExecutedEvent> context)
    {
        var @event = context.Message;

        var exists = await _context.FactAuctions
            .AnyAsync(f => f.AuctionId == @event.AuctionId && f.EventType == AnalyticsEventTypes.BuyNowExecuted,
                context.CancellationToken);

        if (exists)
        {
            _logger.LogWarning(
                "Duplicate BuyNowExecuted event skipped for Auction {AuctionId}", @event.AuctionId);
            return;
        }

        var now = DateTimeOffset.UtcNow;

        var fact = new FactAuction
        {
            EventId = Guid.NewGuid(),
            AuctionId = @event.AuctionId,
            EventTime = @event.ExecutedAt,
            IngestedAt = now,
            WinnerId = @event.BuyerId,
            DateKey = DateOnly.FromDateTime(@event.ExecutedAt.UtcDateTime),

            Title = @event.ItemTitle,
            SellerUsername = @event.Seller,
            WinnerUsername = @event.Buyer,

            FinalPrice = @event.BuyNowPrice,
            BuyNowPrice = @event.BuyNowPrice,

            Status = AnalyticsAuctionStatuses.Sold,
            Sold = true,
            UsedBuyNow = true,
            EndedAt = @event.ExecutedAt,

            EventType = AnalyticsEventTypes.BuyNowExecuted,
            EventVersion = (short)@event.Version
        };

        _context.FactAuctions.Add(fact);
        await _context.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation(
            "Recorded BuyNowExecuted fact for {AuctionId}, Buyer: {Buyer}, Price: {Price}",
            @event.AuctionId, @event.Buyer, @event.BuyNowPrice);
    }
}
