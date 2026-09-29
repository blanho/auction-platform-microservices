using MassTransit;
using Microsoft.EntityFrameworkCore;
using Payment.Domain.Entities;
using Payment.Domain.Enums;
using Payment.Infrastructure.Messaging.Consumers;
using Payment.Infrastructure.Persistence;
using PaymentService.Contracts.Requests;
using TestSupport;
using Xunit;

namespace Payment.Infrastructure.Tests;

public class BuyerPaymentStatusTests
{
    [PostgresFact]
    public async Task PaymentLookupScopesBuyerAndDistinguishesPaidPendingAndRefunded()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        await using var db = new PaymentDbContext(new DbContextOptionsBuilder<PaymentDbContext>()
            .UseNpgsql(database.ConnectionString).Options);
        await db.Database.MigrateAsync();
        var buyer = Guid.NewGuid();
        var paid = Create(buyer);
        paid.CompletePayment("paid-transaction");
        var pending = Create(buyer);
        var refunded = Create(buyer);
        refunded.CompletePayment("refunded-transaction");
        refunded.ChangeStatus(OrderStatus.Disputed);
        refunded.ChangeStatus(OrderStatus.Refunded);
        var foreign = Create(Guid.NewGuid());
        foreign.CompletePayment("other-buyer");
        db.Orders.AddRange(paid, pending, refunded, foreign);
        await db.SaveChangesAsync();
        BuyerAuctionPaymentStatuses? response = null;
        var context = TestProxy.Create<ConsumeContext<GetBuyerAuctionPaymentStatuses>>((method, args) => method.Name switch
        {
            "get_Message" => new GetBuyerAuctionPaymentStatuses(buyer),
            "get_CancellationToken" => CancellationToken.None,
            "RespondAsync" => Respond((BuyerAuctionPaymentStatuses)args![0]!),
            _ => throw new NotSupportedException(method.Name)
        });
        Task Respond(BuyerAuctionPaymentStatuses result) { response = result; return Task.CompletedTask; }
        await new BuyerAuctionPaymentStatusesConsumer(db).Consume(context);
        Assert.NotNull(response);
        Assert.Equal(3, response.Items.Count);
        Assert.True(Assert.Single(response.Items, x => x.AuctionId == paid.AuctionId).IsPaid);
        Assert.Equal("Pending", Assert.Single(response.Items, x => x.AuctionId == pending.AuctionId).Status);
        var refundStatus = Assert.Single(response.Items, x => x.AuctionId == refunded.AuctionId);
        Assert.False(refundStatus.IsPaid);
        Assert.Equal("Refunded", refundStatus.Status);
    }
    private static Order Create(Guid buyer) => Order.Create(Guid.NewGuid(), buyer, "buyer",
        Guid.NewGuid(), "seller", "Item", 10);
}
