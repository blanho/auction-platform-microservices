using Analytics.Api.Data;
using Analytics.Api.Entities;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using PaymentService.Contracts.Events;

namespace Analytics.Api.Consumers;

public class OrderCreatedAnalyticsConsumer : IConsumer<OrderCreatedEvent>
{
    private readonly AnalyticsDbContext _context;
    private readonly IDistributedCache _cache;
    private readonly ILogger<OrderCreatedAnalyticsConsumer> _logger;

    public OrderCreatedAnalyticsConsumer(
        AnalyticsDbContext context,
        IDistributedCache cache,
        ILogger<OrderCreatedAnalyticsConsumer> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var @event = context.Message;

        var now = DateTimeOffset.UtcNow;

        var fact = new FactPayment
        {
            EventId = Guid.NewGuid(),
            EventTime = @event.CreatedAt,
            IngestedAt = now,

            OrderId = @event.OrderId,
            AuctionId = @event.AuctionId,
            BuyerId = @event.BuyerId,
            SellerId = @event.SellerId,
            DateKey = DateOnly.FromDateTime(@event.CreatedAt.UtcDateTime),

            TotalAmount = @event.TotalAmount,

            AuctionTitle = @event.ItemTitle,
            BuyerUsername = @event.BuyerUsername,
            SellerUsername = @event.SellerUsername,

            Status = "Created",
            CreatedAt = @event.CreatedAt,

            IsPaid = false,
            IsRefunded = false,

            EventType = "OrderCreated",
            EventVersion = 1
        };

        _context.FactPayments.Add(fact);
        await _context.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation(
            "Recorded OrderCreated fact: Order={OrderId}, Amount={Amount}",
            @event.OrderId, @event.TotalAmount);
    }
}
