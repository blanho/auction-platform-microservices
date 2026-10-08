using BuildingBlocks.Domain.Exceptions;
using MassTransit;
using OrchestrationService.Contracts.Events;
using Payment.Application.Interfaces;
using Payment.Domain.Entities;
using Payment.Domain.Enums;

namespace Payment.Infrastructure.Messaging.Consumers;

public class CreateAuctionWinnerOrderConsumer(IOrderRepository orders, IUnitOfWork unitOfWork)
    : IConsumer<CreateAuctionWinnerOrder>
{
    public async Task Consume(ConsumeContext<CreateAuctionWinnerOrder> context)
    {
        var m = context.Message;
        try
        {
            var order = await orders.GetByAuctionIdAsync(m.AuctionId, context.CancellationToken);
            if (order is not null && (order.BuyerId != m.WinnerId || order.SellerId != m.SellerId ||
                order.WinningBid != m.Amount || order.Status == OrderStatus.Cancelled || order.BuyNowCorrelationId.HasValue))
                throw new DomainInvariantException("Existing order does not match the auction result");
            if (order is null)
            {
                order = Order.Create(m.AuctionId, m.WinnerId, m.WinnerUsername, m.SellerId,
                    m.SellerUsername, m.ItemTitle, m.Amount);
                await orders.AddAsync(order, context.CancellationToken);
                await unitOfWork.SaveChangesAsync(context.CancellationToken);
            }
            await context.Publish(new AuctionWinnerOrderCreated
            {
                CorrelationId = m.CorrelationId,
                AuctionId = m.AuctionId,
                OrderId = order.Id,
                CreatedAt = order.CreatedAt
            });
        }
        catch (DomainInvariantException ex)
        {
            await context.Publish(new AuctionWinnerOrderFailed
            {
                CorrelationId = m.CorrelationId,
                AuctionId = m.AuctionId,
                Reason = ex.Message
            });
        }
    }
}
