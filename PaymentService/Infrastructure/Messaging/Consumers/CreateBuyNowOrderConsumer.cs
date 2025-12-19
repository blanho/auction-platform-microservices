using Common.Caching.Abstractions;
using Common.Messaging.Events.Saga;
using MassTransit;
using Microsoft.Extensions.Logging;
using PaymentService.Application.Interfaces;
using PaymentService.Domain.Entities;

namespace PaymentService.Infrastructure.Messaging.Consumers;

public class CreateBuyNowOrderConsumer : IConsumer<CreateBuyNowOrder>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly ILogger<CreateBuyNowOrderConsumer> _logger;
    private const string IdempotencyKeyPrefix = "saga:order:";

    public CreateBuyNowOrderConsumer(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        ICacheService cache,
        ILogger<CreateBuyNowOrderConsumer> logger)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<CreateBuyNowOrder> context)
    {
        var message = context.Message;
        var idempotencyKey = $"{IdempotencyKeyPrefix}{message.CorrelationId}";

        _logger.LogInformation(
            "Processing CreateBuyNowOrder - CorrelationId: {CorrelationId}, AuctionId: {AuctionId}",
            message.CorrelationId, message.AuctionId);

        try
        {
            var cachedOrderId = await _cache.GetAsync<string>(idempotencyKey);
            if (!string.IsNullOrEmpty(cachedOrderId))
            {
                _logger.LogInformation(
                    "Order already created for saga - CorrelationId: {CorrelationId}, OrderId: {OrderId}",
                    message.CorrelationId, cachedOrderId);

                await context.Publish(new BuyNowOrderCreated
                {
                    CorrelationId = message.CorrelationId,
                    OrderId = Guid.Parse(cachedOrderId),
                    AuctionId = message.AuctionId,
                    CreatedAt = DateTimeOffset.UtcNow
                });
                return;
            }

            var existingOrder = await _orderRepository.GetByAuctionIdAsync(message.AuctionId);
            if (existingOrder != null)
            {
                _logger.LogWarning(
                    "Order already exists for auction - CorrelationId: {CorrelationId}, AuctionId: {AuctionId}, OrderId: {OrderId}",
                    message.CorrelationId, message.AuctionId, existingOrder.Id);

                await _cache.SetAsync(idempotencyKey, existingOrder.Id.ToString(), TimeSpan.FromHours(24));
                
                await context.Publish(new BuyNowOrderCreated
                {
                    CorrelationId = message.CorrelationId,
                    OrderId = existingOrder.Id,
                    AuctionId = message.AuctionId,
                    CreatedAt = existingOrder.CreatedAt
                });
                return;
            }

            var order = new Order
            {
                Id = Guid.NewGuid(),
                AuctionId = message.AuctionId,
                BuyerId = message.BuyerId,
                BuyerUsername = message.BuyerUsername,
                SellerId = message.SellerId,
                SellerUsername = message.SellerUsername,
                ItemTitle = message.ItemTitle,
                WinningBid = message.BuyNowPrice,
                TotalAmount = message.BuyNowPrice,
                Status = OrderStatus.PendingPayment,
                PaymentStatus = PaymentStatus.Pending
            };

            await _orderRepository.AddAsync(order);
            await _unitOfWork.SaveChangesAsync();

            await _cache.SetAsync(idempotencyKey, order.Id.ToString(), TimeSpan.FromHours(24));

            _logger.LogInformation(
                "Created Buy Now order via saga - CorrelationId: {CorrelationId}, OrderId: {OrderId}, AuctionId: {AuctionId}",
                message.CorrelationId, order.Id, message.AuctionId);

            await context.Publish(new BuyNowOrderCreated
            {
                CorrelationId = message.CorrelationId,
                OrderId = order.Id,
                AuctionId = message.AuctionId,
                CreatedAt = order.CreatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to create order for Buy Now saga - CorrelationId: {CorrelationId}, AuctionId: {AuctionId}",
                message.CorrelationId, message.AuctionId);

            await context.Publish(new BuyNowOrderCreationFailed
            {
                CorrelationId = message.CorrelationId,
                AuctionId = message.AuctionId,
                Reason = $"Failed to create order: {ex.Message}",
                FailedAt = DateTimeOffset.UtcNow
            });
        }
    }
}
