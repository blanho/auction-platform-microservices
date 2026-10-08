using BuildingBlocks.Domain.Exceptions;
using MassTransit;
using OrchestrationService.Contracts.Events;
using Payment.Application.Interfaces;
using Payment.Domain.Enums;

namespace Payment.Infrastructure.Messaging.Consumers;

public class CancelBuyNowOrderConsumer(IOrderRepository orders, IUnitOfWork unitOfWork,
    IBuyNowOrderAttemptStore attempts) : IConsumer<CancelBuyNowOrder>
{
    public async Task Consume(ConsumeContext<CancelBuyNowOrder> context)
    {
        var m = context.Message;
        try
        {
            var attempt = await attempts.GetAsync(m.CorrelationId, m.AuctionId, m.BuyerId, context.CancellationToken);
            var order = await orders.GetByAuctionIdAsync(m.AuctionId, context.CancellationToken);
            var owned = order?.BuyNowCorrelationId == m.CorrelationId && order.BuyerId == m.BuyerId;
            if (owned && order!.Status != OrderStatus.Cancelled)
            {
                order.Cancel(m.Reason);
                await orders.UpdateAsync(order, context.CancellationToken);
            }
            attempt.Cancelled = true;
            await unitOfWork.SaveChangesAsync(context.CancellationToken);
            await context.Publish(new BuyNowOrderCancelled
            {
                CorrelationId = m.CorrelationId,
                AuctionId = m.AuctionId,
                OrderId = owned ? order!.Id : null
            });
        }
        catch (Exception ex) when (ex is DomainInvariantException or InvalidEntityStateException)
        {
            await context.Publish(new BuyNowCompensationFailed
            {
                CorrelationId = m.CorrelationId,
                AuctionId = m.AuctionId,
                Reason = ex.Message
            });
        }
    }
}
