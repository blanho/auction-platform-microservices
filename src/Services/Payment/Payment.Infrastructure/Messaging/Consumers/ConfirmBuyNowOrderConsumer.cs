using MassTransit;
using OrchestrationService.Contracts.Events;
using Payment.Application.Interfaces;

namespace Payment.Infrastructure.Messaging.Consumers;

public class ConfirmBuyNowOrderConsumer(IOrderRepository orders, IUnitOfWork unitOfWork) : IConsumer<ConfirmBuyNowOrder>
{
    public async Task Consume(ConsumeContext<ConfirmBuyNowOrder> context)
    {
        var order = await orders.GetByIdAsync(context.Message.OrderId, context.CancellationToken)
            ?? throw new InvalidOperationException("Completed purchase order is missing");
        if (order.AuctionId != context.Message.AuctionId)
            throw new InvalidOperationException("Completed purchase auction does not match");
        order.ConfirmBuyNow(context.Message.CorrelationId);
        await orders.UpdateAsync(order, context.CancellationToken);
        await unitOfWork.SaveChangesAsync(context.CancellationToken);
        await context.Publish(new BuyNowOrderConfirmed
        {
            CorrelationId = context.Message.CorrelationId,
            AuctionId = order.AuctionId,
            OrderId = order.Id
        });
    }
}
