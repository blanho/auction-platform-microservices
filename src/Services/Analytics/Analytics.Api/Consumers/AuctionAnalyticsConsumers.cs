using Analytics.Api.Data;
using Analytics.Api.Entities;
using AuctionService.Contracts.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Analytics.Api.Consumers;

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
            .AnyAsync(f => f.AuctionId == @event.Id && f.EventType == "Created",
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

            EventType = "Created",
            EventVersion = (short)@event.Version
        };

        _context.FactAuctions.Add(fact);
        await _context.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation(
            "Recorded AuctionCreated fact for {AuctionId}, Seller: {Seller}",
            @event.Id, @event.SellerUsername);
    }
}

public class AuctionFinishedAnalyticsConsumer : IConsumer<AuctionFinishedEvent>
{
    private readonly AnalyticsDbContext _context;
    private readonly ILogger<AuctionFinishedAnalyticsConsumer> _logger;

    public AuctionFinishedAnalyticsConsumer(
        AnalyticsDbContext context,
        ILogger<AuctionFinishedAnalyticsConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AuctionFinishedEvent> context)
    {
        var @event = context.Message;

        var exists = await _context.FactAuctions
            .AnyAsync(f => f.AuctionId == @event.AuctionId && f.EventType == "Finished",
                context.CancellationToken);

        if (exists)
        {
            _logger.LogWarning(
                "Duplicate AuctionFinished event skipped for Auction {AuctionId}",
                @event.AuctionId);
            return;
        }

        var now = DateTimeOffset.UtcNow;

        var fact = new FactAuction
        {
            EventId = Guid.NewGuid(),
            AuctionId = @event.AuctionId,
            EventTime = now,
            IngestedAt = now,
            SellerId = @event.SellerId,
            WinnerId = @event.WinnerId,
            DateKey = DateOnly.FromDateTime(now.UtcDateTime),

            Title = string.Empty,
            SellerUsername = @event.SellerUsername,
            WinnerUsername = @event.WinnerUsername,

            FinalPrice = @event.SoldAmount,

            Status = @event.ItemSold ? "Sold" : "Ended",
            Sold = @event.ItemSold,
            EndedAt = now,

            EventType = "Finished",
            EventVersion = (short)@event.Version
        };

        _context.FactAuctions.Add(fact);
        await _context.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation(
            "Recorded AuctionFinished fact for {AuctionId}, Sold: {Sold}, Amount: {Amount}",
            @event.AuctionId, @event.ItemSold, @event.SoldAmount);
    }
}
