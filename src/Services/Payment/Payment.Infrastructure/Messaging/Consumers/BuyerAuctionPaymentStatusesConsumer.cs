using MassTransit;
using Microsoft.EntityFrameworkCore;
using Payment.Domain.Enums;
using Payment.Infrastructure.Persistence;
using PaymentService.Contracts.Requests;

namespace Payment.Infrastructure.Messaging.Consumers;

public class BuyerAuctionPaymentStatusesConsumer(PaymentDbContext db)
    : IConsumer<GetBuyerAuctionPaymentStatuses>
{
    public async Task Consume(ConsumeContext<GetBuyerAuctionPaymentStatuses> context)
    {
        var orders = await db.Orders.AsNoTracking()
            .Where(o => !o.IsDeleted && o.BuyerId == context.Message.BuyerId)
            .OrderByDescending(o => o.CreatedAt).ThenByDescending(o => o.Id)
            .Select(o => new { o.AuctionId, o.Status, o.PaymentStatus })
            .ToListAsync(context.CancellationToken);
        var statuses = orders.DistinctBy(o => o.AuctionId)
            .Select(o => new AuctionPaymentStatus(
                o.AuctionId,
                o.Status is OrderStatus.Refunded or OrderStatus.Cancelled
                    ? o.Status.ToString()
                    : o.PaymentStatus.ToString(),
                o.PaymentStatus == PaymentStatus.Completed &&
                o.Status is not (OrderStatus.Refunded or OrderStatus.Cancelled)))
            .ToList();
        await context.RespondAsync(new BuyerAuctionPaymentStatuses(statuses));
    }
}
