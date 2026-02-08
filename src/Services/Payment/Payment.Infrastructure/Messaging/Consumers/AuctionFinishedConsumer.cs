using MassTransit;
using Microsoft.Extensions.Logging;
using BuildingBlocks.Infrastructure.Caching;
using AuctionService.Contracts.Events;
using Payment.Application.Interfaces;
using Payment.Domain.Entities;
using IUnitOfWork = BuildingBlocks.Application.Abstractions.IUnitOfWork;

namespace Payment.Infrastructure.Messaging.Consumers;

public class AuctionFinishedConsumer : IConsumer<AuctionFinishedEvent>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AuctionFinishedConsumer> _logger;

    public AuctionFinishedConsumer(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        ILogger<AuctionFinishedConsumer> logger)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AuctionFinishedEvent> context)
    {
        var message = context.Message;
        
        if (!message.ItemSold || string.IsNullOrEmpty(message.WinnerUsername))
        {
            _logger.LogInformation("Auction {AuctionId} ended without sale, skipping order creation", 
                message.AuctionId);
            return;
        }

        var existingOrder = await _orderRepository.GetByAuctionIdAsync(message.AuctionId);
        if (existingOrder != null)
        {
            _logger.LogWarning("Order already exists for auction {AuctionId}", message.AuctionId);
            return;
        }

        var order = Order.Create(
            auctionId: message.AuctionId,
            buyerId: message.WinnerId ?? Guid.Empty,
            buyerUsername: message.WinnerUsername,
            sellerId: message.SellerId,
            sellerUsername: message.SellerUsername,
            itemTitle: message.ItemTitle,
            winningBid: message.SoldAmount ?? 0);

        await _orderRepository.AddAsync(order);
        await _unitOfWork.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation(
            "Created order {OrderId} for auction {AuctionId}, buyer: {BuyerId} ({BuyerUsername}), seller: {SellerId}, amount: {Amount}",
            order.Id, message.AuctionId, message.WinnerId, message.WinnerUsername, message.SellerId, message.SoldAmount);
    }
}
