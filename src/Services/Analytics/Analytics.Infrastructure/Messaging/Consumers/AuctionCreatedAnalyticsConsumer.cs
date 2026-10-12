using Microsoft.EntityFrameworkCore;
using MassTransit;
using Microsoft.Extensions.Logging;
using Analytics.Domain.Constants;
using Analytics.Infrastructure.Persistence;
using Analytics.Domain.Entities;
using AuctionService.Contracts.Events;

namespace Analytics.Infrastructure.Messaging.Consumers;

public class AuctionCreatedAnalyticsConsumer : IConsumer<AuctionCreatedEvent>
{
    private readonly AnalyticsDbContext _context;
    private readonly ILogger<AuctionCreatedAnalyticsConsumer> _logger;

    public AuctionCreatedAnalyticsConsumer(
        AnalyticsDbContext context,
        ILogger<AuctionCreatedAnalyticsConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AuctionCreatedEvent> context)
    {
        var @event = context.Message;

        var exists = await _context.FactAuctions
            .AnyAsync(f => f.AuctionId == @event.Id && f.EventType == AnalyticsEventTypes.Created,
                context.CancellationToken);

        if (exists)
        {
            _logger.LogWarning(
                "Duplicate AuctionCreated event skipped for Auction {AuctionId}",
                @event.Id);
            return;
        }

        var fact = new FactAuction
        {
            EventId = Guid.NewGuid(),
            AuctionId = @event.Id,
            EventTime = @event.CreatedAt,
            IngestedAt = DateTimeOffset.UtcNow,
            SellerId = @event.SellerId,
            DateKey = DateOnly.FromDateTime(@event.CreatedAt.UtcDateTime),

            Title = @event.Title,
            SellerUsername = @event.SellerUsername,

            StartingPrice = @event.ReservePrice,
            ReservePrice = @event.ReservePrice,

            Status = @event.Status,
            CreatedAt = @event.CreatedAt,
            Currency = @event.Currency,
            Condition = @event.Condition,
            HadReserve = @event.ReservePrice > 0,

            EventType = AnalyticsEventTypes.Created,
            EventVersion = (short)@event.Version
        };

        _context.FactAuctions.Add(fact);
        await _context.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation(
            "Recorded AuctionCreated fact for {AuctionId}, Seller: {Seller}",
            @event.Id, @event.SellerUsername);
    }
}
