using Auctions.Infrastructure.Messaging.Consumers;
using Auctions.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using TestSupport;
using Xunit;

namespace Auction.Infrastructure.Tests;

public class WorkflowReceiptPersistenceTests
{
    [PostgresFact]
    public async Task FinalNumberedBatchArrivingFirstDoesNotCompleteImport()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        var options = new DbContextOptionsBuilder<AuctionDbContext>().UseNpgsql(database.ConnectionString).Options;
        var correlation = Guid.NewGuid();
        await using (var db = new AuctionDbContext(options))
        {
            await db.Database.MigrateAsync();
            var store = new AuctionWorkflowStore(db);
            Assert.Null(await store.CompleteAsync("batch-2", correlation, 3, 1, [], 2, default));
        }
        await using (var db = new AuctionDbContext(options))
        {
            var store = new AuctionWorkflowStore(db);
            Assert.True(await store.ExistsAsync("batch-2", default));
            var totals = await store.CompleteAsync("batch-1", correlation, 5, 1, [], 2, default);
            Assert.NotNull(totals);
            Assert.Equal(8, totals.Succeeded);
            Assert.Equal(2, totals.Failed);
        }
    }

    [PostgresFact]
    public async Task ReceiptRollbackAllowsRetryInsteadOfFalseCompletion()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        var options = new DbContextOptionsBuilder<AuctionDbContext>().UseNpgsql(database.ConnectionString).Options;
        await using (var db = new AuctionDbContext(options))
        {
            await db.Database.MigrateAsync();
            await using var transaction = await db.Database.BeginTransactionAsync();
            await new AuctionWorkflowStore(db).CompleteAsync("batch", Guid.NewGuid(), 1, 0, [], 1, default);
            await transaction.RollbackAsync();
        }
        await using var check = new AuctionDbContext(options);
        Assert.False(await new AuctionWorkflowStore(check).ExistsAsync("batch", default));
    }
}
