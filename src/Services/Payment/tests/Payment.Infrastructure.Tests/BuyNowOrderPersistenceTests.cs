using Microsoft.EntityFrameworkCore;
using Payment.Domain.Entities;
using Payment.Infrastructure.Messaging.Consumers;
using Payment.Infrastructure.Persistence;
using TestSupport;
using Xunit;

namespace Payment.Infrastructure.Tests;

public class BuyNowOrderPersistenceTests
{
    [PostgresFact]
    public async Task CommittedCancellationSurvivesRestartAndRollbackDoesNotLeaveAFence()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        var options = new DbContextOptionsBuilder<PaymentDbContext>().UseNpgsql(database.ConnectionString).Options;
        var correlation = Guid.NewGuid(); var rolledBack = Guid.NewGuid(); var auction = Guid.NewGuid(); var buyer = Guid.NewGuid();
        await using (var db = new PaymentDbContext(options))
        {
            await db.Database.MigrateAsync();
            await using var tx = await db.Database.BeginTransactionAsync();
            var attempt = await new BuyNowOrderAttemptStore(db).GetAsync(correlation, auction, buyer, default);
            attempt.Cancelled = true; await db.SaveChangesAsync(); await tx.CommitAsync();
        }
        await using (var db = new PaymentDbContext(options))
        {
            await using var tx = await db.Database.BeginTransactionAsync();
            var store = new BuyNowOrderAttemptStore(db);
            Assert.True((await store.GetAsync(correlation, auction, buyer, default)).Cancelled);
            (await store.GetAsync(rolledBack, auction, buyer, default)).Cancelled = true;
            await db.SaveChangesAsync(); await tx.RollbackAsync();
        }
        await using var check = new PaymentDbContext(options);
        Assert.True(await check.Set<BuyNowOrderAttempt>().AnyAsync(x => x.CorrelationId == correlation && x.Cancelled));
        Assert.False(await check.Set<BuyNowOrderAttempt>().AnyAsync(x => x.CorrelationId == rolledBack));
    }

    [PostgresFact]
    public async Task UniqueIndexAllowsCancelledHistoryButOnlyOneActiveOrder()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        var options = new DbContextOptionsBuilder<PaymentDbContext>().UseNpgsql(database.ConnectionString).Options;
        await using var db = new PaymentDbContext(options);
        await db.Database.MigrateAsync();
        var auction = Guid.NewGuid();
        Order NewOrder() => Order.Create(auction, Guid.NewGuid(), "buyer", Guid.NewGuid(), "seller", "item", 100,
            buyNowCorrelationId: Guid.NewGuid());
        var cancelled = NewOrder(); cancelled.Cancel(); db.Add(cancelled); await db.SaveChangesAsync();
        var active = NewOrder(); db.Add(active); await db.SaveChangesAsync();
        db.Add(NewOrder());
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }
}
