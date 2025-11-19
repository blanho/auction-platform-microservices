using Analytics.Api.Data;
using Analytics.Api.Entities;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using PaymentService.Contracts.Events;

namespace Analytics.Api.Consumers;

public class OrderShippedAnalyticsConsumer : IConsumer<OrderShippedEvent>
{
    private readonly AnalyticsDbContext _context;
    private readonly IDistributedCache _cache;
    private readonly ILogger<OrderShippedAnalyticsConsumer> _logger;

    public OrderShippedAnalyticsConsumer(
        AnalyticsDbContext context,
        IDistributedCache cache,
        ILogger<OrderShippedAnalyticsConsumer> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderShippedEvent> context)
    {
        var @event = context.Message;

        var now = DateTimeOffset.UtcNow;

        var fact = new FactPayment
        {
            EventId = Guid.NewGuid(),
            EventTime = @event.ShippedAt,
            IngestedAt = now,

            OrderId = @event.OrderId,
            AuctionId = @event.AuctionId,
            BuyerId = @event.BuyerId,
            DateKey = DateOnly.FromDateTime(@event.ShippedAt.UtcDateTime),

            BuyerUsername = @event.BuyerUsername,

            Status = "Shipped",
            ShippedAt = @event.ShippedAt,
            TrackingNumber = @event.TrackingNumber,
            ShippingCarrier = @event.ShippingCarrier,

            IsPaid = true,
            IsRefunded = false,

            EventType = "OrderShipped",
            EventVersion = (short)@event.Version
        };

        _context.FactPayments.Add(fact);
        await _context.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation(
            "Recorded OrderShipped fact: Order={OrderId}, Carrier={Carrier}",
            @event.OrderId, @event.ShippingCarrier);
    }
}
