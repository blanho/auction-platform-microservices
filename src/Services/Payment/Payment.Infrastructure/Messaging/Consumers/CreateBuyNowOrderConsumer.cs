using MassTransit;
using Microsoft.Extensions.Logging;
using BuildingBlocks.Infrastructure.Caching;
using BuildingBlocks.Infrastructure.Repository;
using Orchestration.Sagas.BuyNow.Events;
using Payment.Application.Interfaces;
using Payment.Domain.Entities;

namespace Payment.Infrastructure.Messaging.Consumers;

public class CreateBuyNowOrderConsumer : IConsumer<CreateBuyNowOrder>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateBuyNowOrderConsumer> _logger;

    public CreateBuyNowOrderConsumer(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateBuyNowOrderConsumer> logger)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<CreateBuyNowOrder> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Processing CreateBuyNowOrder - CorrelationId: {CorrelationId}, AuctionId: {AuctionId}",
            message.CorrelationId, message.AuctionId);

        try
        {
            var existingOrder = await _orderRepository.GetByAuctionIdAsync(message.AuctionId);
            if (existingOrder != null)
            {
                _logger.LogWarning(
                    "Order already exists for auction - CorrelationId: {CorrelationId}, AuctionId: {AuctionId}, OrderId: {OrderId}",
                    message.CorrelationId, message.AuctionId, existingOrder.Id);
                
                await context.Publish(new BuyNowOrderCreated
                {
                    CorrelationId = message.CorrelationId,
                    OrderId = existingOrder.Id,
                    AuctionId = message.AuctionId,
                    CreatedAt = existingOrder.CreatedAt
                });
                return;
            }

            var order = Order.Create(
                auctionId: message.AuctionId,
                buyerId: message.BuyerId,
                buyerUsername: message.BuyerUsername,
                sellerId: message.SellerId,
                sellerUsername: message.SellerUsername,
                itemTitle: message.ItemTitle,
                winningBid: message.BuyNowPrice);

            await _orderRepository.AddAsync(order);
            await _unitOfWork.SaveChangesAsync(context.CancellationToken);

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
