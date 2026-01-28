using Analytics.Api.Data;
using Analytics.Api.Entities;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using PaymentService.Contracts.Events;

namespace Analytics.Api.Consumers;

public class PaymentCompletedAnalyticsConsumer : IConsumer<PaymentCompletedEvent>
{
    private readonly AnalyticsDbContext _context;
    private readonly IDistributedCache _cache;
    private readonly ILogger<PaymentCompletedAnalyticsConsumer> _logger;

    public PaymentCompletedAnalyticsConsumer(
        AnalyticsDbContext context,
        IDistributedCache cache,
        ILogger<PaymentCompletedAnalyticsConsumer> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentCompletedEvent> context)
    {
        var @event = context.Message;

        var now = DateTimeOffset.UtcNow;

        var fact = new FactPayment
        {
            EventId = Guid.NewGuid(),
            EventTime = @event.PaidAt,
            IngestedAt = now,

            OrderId = @event.OrderId,
            AuctionId = @event.AuctionId,
            BuyerId = @event.BuyerId,
            SellerId = @event.SellerId,
            DateKey = DateOnly.FromDateTime(@event.PaidAt.UtcDateTime),

            TotalAmount = @event.Amount,

            BuyerUsername = @event.BuyerUsername,
            SellerUsername = @event.SellerUsername,

            Status = "Paid",
            PaidAt = @event.PaidAt,
            TransactionId = @event.TransactionId,

            IsPaid = true,
            IsRefunded = false,

            EventType = "PaymentCompleted",
            EventVersion = 1
        };

        _context.FactPayments.Add(fact);
        await _context.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation(
            "Recorded PaymentCompleted fact: Order={OrderId}, Amount={Amount}",
            @event.OrderId, @event.Amount);
    }
}
