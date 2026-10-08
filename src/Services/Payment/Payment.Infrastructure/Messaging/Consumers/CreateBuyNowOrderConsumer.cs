using BuildingBlocks.Domain.Exceptions;
using Payment.Domain.Enums;
using MassTransit;
using Microsoft.Extensions.Logging;
using BuildingBlocks.Infrastructure.Caching;
using OrchestrationService.Contracts.Events;
using Payment.Application.Interfaces;
using Payment.Domain.Entities;

namespace Payment.Infrastructure.Messaging.Consumers;

public class CreateBuyNowOrderConsumer : IConsumer<CreateBuyNowOrder>
{
    private readonly IBuyNowOrderAttemptStore _attempts;
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateBuyNowOrderConsumer> _logger;

    public CreateBuyNowOrderConsumer(
        IOrderRepository orderRepository,
        IBuyNowOrderAttemptStore attempts,
        IUnitOfWork unitOfWork,
        ILogger<CreateBuyNowOrderConsumer> logger)
    {
        _orderRepository = orderRepository;
        _attempts = attempts;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<CreateBuyNowOrder> context)
    {
        context.CancellationToken.ThrowIfCancellationRequested();
        var message = context.Message;

        _logger.LogInformation(
            "Processing CreateBuyNowOrder - CorrelationId: {CorrelationId}, AuctionId: {AuctionId}",
            message.CorrelationId, message.AuctionId);

        try
        {
            var attempt = await _attempts.GetAsync(message.CorrelationId, message.AuctionId, message.BuyerId, context.CancellationToken);
            if (attempt.Cancelled)
                throw new DomainInvariantException("This purchase attempt was cancelled");
            var existingOrder = await _orderRepository.GetByAuctionIdAsync(message.AuctionId, context.CancellationToken);
            if (existingOrder?.Status == OrderStatus.Cancelled) existingOrder = null;
            if (existingOrder != null)
            {
                if (existingOrder.BuyNowCorrelationId != message.CorrelationId || existingOrder.BuyerId != message.BuyerId ||
                    existingOrder.SellerId != message.SellerId || existingOrder.WinningBid != message.BuyNowPrice)
                    throw new DomainInvariantException("An order already exists for a different purchase attempt");
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
                winningBid: message.BuyNowPrice,
                buyNowCorrelationId: message.CorrelationId);

            await _orderRepository.AddAsync(order, context.CancellationToken);
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
        catch (DomainInvariantException ex)
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
