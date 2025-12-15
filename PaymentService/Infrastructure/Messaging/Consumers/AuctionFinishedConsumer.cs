using MassTransit;
using Microsoft.Extensions.Logging;
using Common.Caching.Abstractions;
using Common.Messaging.Events;
using PaymentService.Application.Interfaces;
using PaymentService.Domain.Entities;

namespace PaymentService.Infrastructure.Messaging.Consumers;

public class AuctionFinishedConsumer : IConsumer<AuctionFinishedEvent>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly ILogger<AuctionFinishedConsumer> _logger;
    private const string IdempotencyKeyPrefix = "msg:auction-finished:";

    public AuctionFinishedConsumer(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        ICacheService cache,
        ILogger<AuctionFinishedConsumer> logger)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AuctionFinishedEvent> context)
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
        
        if (!message.ItemSold || string.IsNullOrEmpty(message.Winner))
        {
            _logger.LogInformation("Auction {AuctionId} ended without sale, skipping order creation", 
                message.AuctionId);
            await _cache.SetAsync(idempotencyKey, "skipped", TimeSpan.FromHours(24));
            return;
        }

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
            BuyerUsername = message.Winner,
            SellerUsername = message.Seller,
            WinningBid = message.SoldAmount ?? 0,
            TotalAmount = message.SoldAmount ?? 0,
            Status = OrderStatus.PendingPayment,
            PaymentStatus = PaymentStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _orderRepository.AddAsync(order);
        await _unitOfWork.SaveChangesAsync();

        await _cache.SetAsync(idempotencyKey, "processed", TimeSpan.FromHours(24));

        _logger.LogInformation(
            "Created order {OrderId} for auction {AuctionId}, buyer: {Buyer}, amount: {Amount}",
            order.Id, message.AuctionId, message.Winner, message.SoldAmount);
    }
}
