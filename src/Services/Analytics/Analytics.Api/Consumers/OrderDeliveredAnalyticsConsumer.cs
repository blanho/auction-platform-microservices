using Analytics.Api.Data;
using Analytics.Api.Entities;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using PaymentService.Contracts.Events;

namespace Analytics.Api.Consumers;

public class OrderDeliveredAnalyticsConsumer : IConsumer<OrderDeliveredEvent>
{
    private readonly AnalyticsDbContext _context;
    private readonly IDistributedCache _cache;
    private readonly ILogger<OrderDeliveredAnalyticsConsumer> _logger;

    public OrderDeliveredAnalyticsConsumer(
        AnalyticsDbContext context,
        IDistributedCache cache,
        ILogger<OrderDeliveredAnalyticsConsumer> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderDeliveredEvent> context)
    {
        var @event = context.Message;

        var now = DateTimeOffset.UtcNow;

        var fact = new FactPayment
        {
            EventId = Guid.NewGuid(),
            EventTime = @event.DeliveredAt,
            IngestedAt = now,

            OrderId = @event.OrderId,
            AuctionId = @event.AuctionId,
            BuyerId = @event.BuyerId,
            SellerId = @event.SellerId,
            DateKey = DateOnly.FromDateTime(@event.DeliveredAt.UtcDateTime),

            BuyerUsername = @event.BuyerUsername,
            SellerUsername = @event.SellerUsername,

            Status = "Delivered",
            DeliveredAt = @event.DeliveredAt,

            IsPaid = true,
            IsRefunded = false,

            EventType = "OrderDelivered",
            EventVersion = (short)@event.Version
        };

        _context.FactPayments.Add(fact);
        await _context.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation(
            "Recorded OrderDelivered fact: Order={OrderId}",
            @event.OrderId);
    }
}
