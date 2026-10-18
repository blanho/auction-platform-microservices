using Payment.Application.Features.Orders.GetOrdersByBuyer;
using Payment.Application.Features.Orders.GetOrdersBySeller;
using Payment.Application.Filtering;
using Payment.Domain.Entities;
using Payment.Infrastructure.Persistence;
using Payment.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using TestSupport;
using Xunit;

namespace Payment.Infrastructure.Tests;

public sealed class OrderPagingTests
{
    [PostgresFact]
    public async Task OrderPaging_FiltersBeforeCountAndOrdersEqualDatesById()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        await using var db = new PaymentDbContext(new DbContextOptionsBuilder<PaymentDbContext>()
            .UseNpgsql(database.ConnectionString).Options);
        await db.Database.MigrateAsync();
        var orders = Enumerable.Range(0, 3).Select(index => Order.Create(Guid.NewGuid(), Guid.NewGuid(),
            index == 2 ? "other-buyer" : "buyer", Guid.NewGuid(), "seller", "item", 100m)).ToArray();
        var timestamp = DateTimeOffset.UtcNow.AddDays(-1);
        foreach (var order in orders) order.SetCreatedAudit(Guid.Empty, timestamp);
        db.Orders.AddRange(orders);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var repository = new OrderRepository(db);
        OrderQueryParams Parameters(int page) => new()
        {
            Page = page, PageSize = 1, Filter = new OrderFilter { BuyerUsername = "buyer" }
        };

        var first = await repository.GetByBuyerUsernameAsync(Parameters(1));
        var second = await repository.GetByBuyerUsernameAsync(Parameters(2));
        Assert.Equal(2, first.TotalCount);
        Assert.Equal(orders.Take(2).OrderBy(order => order.Id).Select(order => order.Id),
            first.Items.Concat(second.Items).Select(order => order.Id));
        Assert.Empty(db.ChangeTracker.Entries());

        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => new GetOrdersByBuyerQueryHandler(repository, null!)
            .Handle(new GetOrdersByBuyerQuery("buyer"), cancellation.Token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => new GetOrdersBySellerQueryHandler(repository, null!)
            .Handle(new GetOrdersBySellerQuery("seller"), cancellation.Token));
    }
}
