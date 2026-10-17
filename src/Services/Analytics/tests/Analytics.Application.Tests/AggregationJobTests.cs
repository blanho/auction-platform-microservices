using Analytics.Domain.Constants;
using Analytics.Domain.Entities;
using Analytics.Infrastructure.Messaging.Consumers;
using Analytics.Infrastructure.Persistence;
using Analytics.Infrastructure.Repositories;
using AnalyticsService.Contracts.Commands;
using JobService.Contracts.Commands;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using TestSupport;
using Xunit;

namespace Analytics.Application.Tests;

public class AggregationJobTests
{
    private static readonly DateOnly Day = new(2026, 1, 15);

    [PostgresFact]
    public async Task RebuildAggregatesFactsAndReplaysWithoutDoubleCounting()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        await using var db = Open(database);
        await db.Database.MigrateAsync();
        var seller = Guid.NewGuid();
        var buyer = Guid.NewGuid();
        var auction = Guid.NewGuid();
        db.FactAuctions.AddRange(
            Auction(auction, seller, buyer, AnalyticsEventTypes.Created, false, null),
            Auction(auction, seller, buyer, AnalyticsEventTypes.BuyNowExecuted, true, 100),
            Auction(auction, seller, buyer, AnalyticsEventTypes.Finished, true, 100),
            Auction(Guid.NewGuid(), seller, null, AnalyticsEventTypes.Finished, false, null));
        db.FactBids.AddRange(Bid(auction, buyer, 20), Bid(auction, buyer, 40));
        db.FactPayments.AddRange(
            Payment(seller, buyer, AnalyticsEventTypes.OrderCreated, 100),
            Payment(seller, buyer, AnalyticsEventTypes.PaymentCompleted, 100),
            Payment(seller, buyer, AnalyticsEventTypes.OrderShipped, 100),
            Payment(seller, buyer, "Refunded", 25, true));
        await db.SaveChangesAsync();
        var command = Command();
        for (var i = 0; i < 2; i++)
        {
            var result = Assert.Single(await Run(db, command));
            Assert.True(result.IsSuccess);
            Assert.Equal(command.ParentJobId, result.JobId);
            Assert.Equal(command.ParentJobItemId, result.JobItemId);
            Assert.Equal(2, result.Attempt);
        }
        var stats = await new DailyStatsRepository(db).GetAggregatedStatsAsync(Day, Day);
        Assert.Equal(100, stats.Summary.TotalAuctionRevenue);
        Assert.Equal(100, stats.Summary.TotalRevenue);
        Assert.Equal(25, stats.Summary.TotalRefunds);
        var finished = Assert.Single(stats.AuctionStats, s => s.EventType == AnalyticsEventTypes.Finished);
        Assert.Equal(2, finished.EventCount);
        Assert.Equal(1, finished.UniqueSellers);
        Assert.Equal(1, finished.SoldCount);
        Assert.Equal(1, finished.UnsoldCount);
        var bids = Assert.Single(stats.BidStats);
        Assert.Equal(2, bids.TotalBids);
        Assert.Equal(1, bids.UniqueBidders);
        Assert.Equal(60, bids.TotalBidValue);
        Assert.Equal(30, bids.AvgBidAmount);
        // A late fact is included on the next run, while unrelated dates are retained.
        db.FactBids.Add(Bid(auction, buyer, 60));
        var outside = Bid(auction, buyer, 90); outside.DateKey = Day.AddDays(-1);
        db.FactBids.Add(outside);
        await db.SaveChangesAsync();
        await Run(db, command with { StartDate = Day.AddDays(-1) });
        await Run(db, command);
        Assert.Equal(2, await db.DailyBidStats.CountAsync());
        Assert.Equal(120, (await db.DailyBidStats.AsNoTracking().SingleAsync(s => s.DateKey == Day)).TotalBidValue);
        // Removing all source rows also removes obsolete aggregate rows for the selected date.
        await db.FactBids.Where(b => b.DateKey == Day).ExecuteDeleteAsync();
        await Run(db, command);
        Assert.Equal(Day.AddDays(-1), (await db.DailyBidStats.AsNoTracking().SingleAsync()).DateKey);
    }

    [PostgresFact]
    public async Task InvalidPeriodsDoNotModifyExistingTotals()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        await using var db = Open(database);
        await db.Database.MigrateAsync();
        db.FactBids.Add(Bid(Guid.NewGuid(), Guid.NewGuid(), 10));
        await db.SaveChangesAsync();
        await Run(db, Command());
        var command = Command();
        foreach (var invalid in new[] { command with { StartDate = default }, command with { EndDate = Day.AddDays(-1) },
                     command with { EndDate = Day.AddDays(31) }, command with { RequestedBy = Guid.Empty },
                     command with { StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)), EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)) } })
        {
            var result = Assert.Single(await Run(db, invalid));
            Assert.False(result.IsSuccess);
            Assert.True(result.IsFinalFailure);
        }
        Assert.Equal(10, (await db.DailyBidStats.SingleAsync()).TotalBidValue);
        Assert.True(Assert.Single(await Run(db, command with { EndDate = Day.AddDays(30) })).IsSuccess);
    }

    [PostgresFact]
    public async Task FailedPublicationRollsBackReplacementAndCancellationHasNoResult()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        await using var db = Open(database);
        await db.Database.MigrateAsync();
        db.FactBids.Add(Bid(Guid.NewGuid(), Guid.NewGuid(), 10));
        await db.SaveChangesAsync();
        await Run(db, Command());
        db.FactBids.Add(Bid(Guid.NewGuid(), Guid.NewGuid(), 20));
        await db.SaveChangesAsync();
        await Assert.ThrowsAsync<IOException>(() => Run(db, Command(), failPublish: true));
        Assert.Equal(10, (await db.DailyBidStats.AsNoTracking().SingleAsync()).TotalBidValue);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Run(db, Command(), ct: new CancellationToken(true)));
    }

    [PostgresFact]
    public async Task ConcurrentOverlappingRunsLeaveOneConsistentSetOfRows()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        await using (var db = Open(database))
        {
            await db.Database.MigrateAsync();
            db.FactBids.Add(Bid(Guid.NewGuid(), Guid.NewGuid(), 10));
            await db.SaveChangesAsync();
        }
        async Task Execute()
        {
            await using var db = Open(database);
            Assert.True(Assert.Single(await Run(db, Command())).IsSuccess);
        }
        await Task.WhenAll(Execute(), Execute());
        await using var read = Open(database);
        Assert.Equal(10, (await read.DailyBidStats.SingleAsync()).TotalBidValue);
    }

    private static AnalyticsDbContext Open(PostgresTestDatabase database) =>
        new(new DbContextOptionsBuilder<AnalyticsDbContext>().UseNpgsql(database.ConnectionString).Options);
    private static AggregateDailyStatsCommand Command() => new()
    {
        StartDate = Day, EndDate = Day, RequestedBy = Guid.NewGuid(),
        ParentJobId = Guid.NewGuid(), ParentJobItemId = Guid.NewGuid(), Attempt = 2
    };
    private static async Task<List<ReportJobItemResultCommand>> Run(AnalyticsDbContext db, AggregateDailyStatsCommand command,
        bool failPublish = false, CancellationToken ct = default)
    {
        var results = new List<ReportJobItemResultCommand>();
        var context = TestProxy.Create<ConsumeContext<AggregateDailyStatsCommand>>((method, args) => method.Name switch
        {
            "get_Message" => command,
            "get_CancellationToken" => ct,
            "Publish" => failPublish ? throw new IOException("Broker unavailable") : Capture(args![0]!),
            _ => throw new NotSupportedException(method.Name)
        });
        await new AggregateDailyStatsConsumer(db).Consume(context);
        return results;
        Task Capture(object value) { results.Add(Assert.IsType<ReportJobItemResultCommand>(value)); return Task.CompletedTask; }
    }
    private static FactAuction Auction(Guid auction, Guid seller, Guid? buyer, string type, bool sold, decimal? price) => new()
    {
        EventId = Guid.NewGuid(), AuctionId = auction, SellerId = seller, WinnerId = buyer,
        DateKey = Day, EventType = type, Sold = sold, FinalPrice = price
    };
    private static FactBid Bid(Guid auction, Guid buyer, decimal amount) => new()
    {
        EventId = Guid.NewGuid(), AuctionId = auction, BidderId = buyer, DateKey = Day, BidAmount = amount
    };
    private static FactPayment Payment(Guid seller, Guid buyer, string type, decimal amount, bool refunded = false) => new()
    {
        EventId = Guid.NewGuid(), OrderId = Guid.NewGuid(), SellerId = seller, BuyerId = buyer,
        DateKey = Day, EventType = type, TotalAmount = amount, IsPaid = type != AnalyticsEventTypes.OrderCreated, IsRefunded = refunded
    };
}
