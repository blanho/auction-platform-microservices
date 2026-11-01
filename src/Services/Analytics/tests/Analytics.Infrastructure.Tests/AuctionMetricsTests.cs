using Analytics.Domain.Constants;
using Analytics.Domain.Entities;
using Analytics.Infrastructure.Persistence;
using Analytics.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using TestSupport;
using Xunit;

namespace Analytics.Infrastructure.Tests;

public class AuctionMetricsTests
{
    [PostgresFact]
    public async Task LatestAuctionQueriesPreservePeriodBoundariesAndLiveStatusCounts()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        await using var db = new AnalyticsDbContext(new DbContextOptionsBuilder<AnalyticsDbContext>()
            .UseNpgsql(database.ConnectionString).Options);
        await db.Database.MigrateAsync();
        var start = new DateTimeOffset(2026, 11, 1, 0, 0, 0, TimeSpan.Zero);
        var end = start.AddDays(1);
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();
        db.FactAuctions.AddRange(
            Fact(first, start, AnalyticsAuctionStatuses.Live),
            Fact(first, end.AddSeconds(1), AnalyticsAuctionStatuses.Cancelled),
            Fact(second, start.AddHours(1), AnalyticsAuctionStatuses.Active));
        await db.SaveChangesAsync();
        var repository = new FactAuctionRepository(db);

        var period = await repository.GetAuctionMetricsAsync(start, end);
        var overall = await repository.GetAuctionMetricsAsync(null, null);
        var live = await repository.GetLiveAuctionsCountAsync();
        var seller = await repository.GetUserAuctionStatsAsync("seller");
        var listings = await repository.GetTopListingsAsync("seller", 10);

        Assert.Equal(2, period.LiveAuctions);
        Assert.Equal(0, period.CancelledAuctions);
        Assert.Equal(1, overall.LiveAuctions);
        Assert.Equal(1, overall.CancelledAuctions);
        Assert.Equal(1, live);
        Assert.Equal(2, seller.TotalAuctions);
        Assert.Equal(1, seller.ActiveAuctions);
        Assert.Equal(second.ToString(), Assert.Single(listings).Id);
    }

    private static FactAuction Fact(Guid auctionId, DateTimeOffset time, string status) => new()
    {
        EventId = Guid.NewGuid(), AuctionId = auctionId, EventTime = time, IngestedAt = time,
        SellerId = Guid.NewGuid(), SellerUsername = "seller", Title = "Item", Status = status,
        DateKey = DateOnly.FromDateTime(time.UtcDateTime), CreatedAt = time,
        EventType = AnalyticsEventTypes.Created, StartingPrice = 10
    };
}
