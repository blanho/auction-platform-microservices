using MassTransit;
using Microsoft.Extensions.Logging;
using BuildingBlocks.Infrastructure.Caching;
using BuildingBlocks.Infrastructure.Repository;
using AuctionService.Contracts.Events;
using Payment.Application.Interfaces;
using Payment.Domain.Entities;

namespace Payment.Infrastructure.Messaging.Consumers;

public class BuyNowExecutedConsumer : IConsumer<BuyNowExecutedEvent>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BuyNowExecutedConsumer> _logger;

    public BuyNowExecutedConsumer(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        ILogger<BuyNowExecutedConsumer> logger)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<BuyNowExecutedEvent> context)
    {
        var message = context.Message;
        
        _logger.LogInformation("Processing BuyNowExecutedEvent for auction {AuctionId}", message.AuctionId);

        var existingOrder = await _orderRepository.GetByAuctionIdAsync(message.AuctionId);
        if (existingOrder != null)
        {
            _logger.LogWarning("Order already exists for auction {AuctionId}", message.AuctionId);
            return;
        }

        var order = Order.Create(
            auctionId: message.AuctionId,
            buyerId: message.BuyerId,
            buyerUsername: message.Buyer,
            sellerId: message.SellerId,
            sellerUsername: message.Seller,
            itemTitle: message.ItemTitle,
            winningBid: message.BuyNowPrice);

        await _orderRepository.AddAsync(order);
        await _unitOfWork.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation(
            "Created Buy Now order {OrderId} for auction {AuctionId}, buyer: {BuyerId} ({BuyerUsername}), seller: {SellerId}, amount: {Amount}",
            order.Id, message.AuctionId, message.BuyerId, message.Buyer, message.SellerId, message.BuyNowPrice);
    }
}
