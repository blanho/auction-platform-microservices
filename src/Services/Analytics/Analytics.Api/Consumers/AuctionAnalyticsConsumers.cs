using Analytics.Api.Constants;
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
            .AnyAsync(f => f.AuctionId == @event.AuctionId && f.EventType == AnalyticsEventTypes.Finished,
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

            Status = @event.ItemSold ? AnalyticsAuctionStatuses.Sold : AnalyticsAuctionStatuses.Ended,
            Sold = @event.ItemSold,
            EndedAt = now,

            EventType = AnalyticsEventTypes.Finished,
            EventVersion = (short)@event.Version
        };

        _context.FactAuctions.Add(fact);
        await _context.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation(
            "Recorded AuctionFinished fact for {AuctionId}, Sold: {Sold}, Amount: {Amount}",
            @event.AuctionId, @event.ItemSold, @event.SoldAmount);
    }
}

public class AuctionStartedAnalyticsConsumer : IConsumer<AuctionStartedEvent>
{
    private readonly AnalyticsDbContext _context;
    private readonly ILogger<AuctionStartedAnalyticsConsumer> _logger;

    public AuctionStartedAnalyticsConsumer(
        AnalyticsDbContext context,
        ILogger<AuctionStartedAnalyticsConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AuctionStartedEvent> context)
    {
        var @event = context.Message;

        var exists = await _context.FactAuctions
            .AnyAsync(f => f.AuctionId == @event.AuctionId && f.EventType == AnalyticsEventTypes.Started,
                context.CancellationToken);

        if (exists)
        {
            _logger.LogWarning(
                "Duplicate AuctionStarted event skipped for Auction {AuctionId}", @event.AuctionId);
            return;
        }

        var now = DateTimeOffset.UtcNow;

        var fact = new FactAuction
        {
            EventId = Guid.NewGuid(),
            AuctionId = @event.AuctionId,
            EventTime = @event.StartTime,
            IngestedAt = now,
            DateKey = DateOnly.FromDateTime(@event.StartTime.UtcDateTime),

            Title = @event.Title,
            SellerUsername = @event.Seller,

            ReservePrice = @event.ReservePrice,
            StartedAt = @event.StartTime,

            Status = AnalyticsAuctionStatuses.Live,
            HadReserve = @event.ReservePrice > 0,

            EventType = AnalyticsEventTypes.Started,
            EventVersion = (short)@event.Version
        };

        _context.FactAuctions.Add(fact);
        await _context.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation(
            "Recorded AuctionStarted fact for {AuctionId}, Seller: {Seller}",
            @event.AuctionId, @event.Seller);
    }
}

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
