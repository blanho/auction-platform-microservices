using MassTransit;
using Microsoft.Extensions.Logging;
using Common.Caching.Abstractions;
using Common.Messaging.Events;
using PaymentService.Application.Interfaces;
using PaymentService.Domain.Entities;

namespace PaymentService.Infrastructure.Messaging.Consumers;

public class BuyNowExecutedConsumer : IConsumer<BuyNowExecutedEvent>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly ILogger<BuyNowExecutedConsumer> _logger;
    private const string IdempotencyKeyPrefix = "msg:buy-now:";

    public BuyNowExecutedConsumer(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        ICacheService cache,
        ILogger<BuyNowExecutedConsumer> logger)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<BuyNowExecutedEvent> context)
    {
        var message = context.Message;
        var messageId = context.MessageId?.ToString() ?? message.AuctionId.ToString();
        var idempotencyKey = $"{IdempotencyKeyPrefix}{messageId}";

        var alreadyProcessed = await _cache.GetAsync<string>(idempotencyKey);
        if (alreadyProcessed != null)
        {
            _logger.LogInformation("Message {MessageId} already processed, skipping", messageId);
            return;
        }
        
        _logger.LogInformation("Processing BuyNowExecutedEvent for auction {AuctionId}", message.AuctionId);

        var existingOrder = await _orderRepository.GetByAuctionIdAsync(message.AuctionId);
        if (existingOrder != null)
        {
            _logger.LogWarning("Order already exists for auction {AuctionId}", message.AuctionId);
            await _cache.SetAsync(idempotencyKey, "exists", TimeSpan.FromHours(24));
            return;
        }

        var order = new Order
        {
            Id = Guid.NewGuid(),
            AuctionId = message.AuctionId,
            BuyerUsername = message.Buyer,
            SellerUsername = message.Seller,
            ItemTitle = message.ItemTitle,
            WinningBid = message.BuyNowPrice,
            TotalAmount = message.BuyNowPrice,
            Status = OrderStatus.PendingPayment,
            PaymentStatus = PaymentStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _orderRepository.AddAsync(order);
        await _unitOfWork.SaveChangesAsync();

        await _cache.SetAsync(idempotencyKey, "processed", TimeSpan.FromHours(24));

        _logger.LogInformation(
            "Created Buy Now order {OrderId} for auction {AuctionId}, buyer: {Buyer}, amount: {Amount}",
            order.Id, message.AuctionId, message.Buyer, message.BuyNowPrice);
    }
}
