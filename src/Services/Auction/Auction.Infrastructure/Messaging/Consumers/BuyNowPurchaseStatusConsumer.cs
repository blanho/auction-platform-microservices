using Auctions.Domain.Entities;
using Auctions.Infrastructure.Persistence;

namespace Auctions.Infrastructure.Messaging.Consumers;

public class BuyNowPurchaseStatusConsumer(AuctionDbContext db) : IConsumer<BuyNowSagaCompleted>, IConsumer<SagaRecoveryRequired>
{
    public async Task Consume(ConsumeContext<BuyNowSagaCompleted> context)
    {
        var purchase = await db.Set<BuyNowPurchase>().SingleOrDefaultAsync(x => x.CorrelationId == context.Message.CorrelationId, context.CancellationToken);
        if (purchase is null) throw new InvalidOperationException("Purchase receipt is missing");
        if (purchase.Status is "Completed" or "Failed") return;
        purchase.Status = context.Message.Success ? "Completed" : "Failed";
        purchase.OrderId = context.Message.OrderId == Guid.Empty ? null : context.Message.OrderId;
        purchase.CompletedAt = context.Message.CompletedAt;
        await db.SaveChangesAsync(context.CancellationToken);
    }
    public async Task Consume(ConsumeContext<SagaRecoveryRequired> context)
    {
        if (context.Message.Workflow != "BuyNow") return;
        var purchase = await db.Set<BuyNowPurchase>().SingleOrDefaultAsync(x => x.CorrelationId == context.Message.CorrelationId, context.CancellationToken);
        if (purchase is null || purchase.Status is "Completed" or "Failed") return;
        purchase.Status = "NeedsReview";
        await db.SaveChangesAsync(context.CancellationToken);
    }
}
