using Bidding.Application.Filtering;
using Bidding.Domain.Entities;
using Bidding.Domain.Enums;
using Bidding.Infrastructure.Persistence;
using Bidding.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using TestSupport;
using Xunit;

namespace Bidding.Infrastructure.Tests;

public sealed class SortingPersistenceTests
{
    [PostgresFact]
    public async Task StatusSort_UsesStoredEnumOrderWithStableTiesAndFilteredPaging()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        await using var db = Open(database);
        await db.Database.MigrateAsync();
        var auctionId = Guid.NewGuid();
        var bids = Enumerable.Range(1, 4).Select(index => Bid.Create(auctionId, Guid.NewGuid(), "buyer",
            index * 10m, DateTimeOffset.UtcNow)).ToArray();
        bids[1].Accept();
        bids[2].Accept();
        bids[3].Reject("test");
        db.Bids.AddRange(bids);
        db.Bids.Add(Bid.Create(Guid.NewGuid(), Guid.NewGuid(), "other", 10m, DateTimeOffset.UtcNow));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var repository = new BidRepository(db, null!, null!);
        var first = await repository.GetBidsForAuctionPagedAsync(new BidQueryParams
        {
            Filter = new BidFilter { AuctionId = auctionId }, SortBy = "STATUS", SortDescending = false, PageSize = 2
        });
        var second = await repository.GetBidsForAuctionPagedAsync(new BidQueryParams
        {
            Filter = new BidFilter { AuctionId = auctionId }, SortBy = "status", SortDescending = false, PageSize = 2, Page = 2
        });

        Assert.Equal(4, first.TotalCount);
        Assert.Equal(new[] { BidStatus.Pending, BidStatus.Accepted, BidStatus.Accepted, BidStatus.Rejected },
            first.Items.Concat(second.Items).Select(bid => bid.Status));
        Assert.Equal(bids.OrderBy(bid => bid.Status).ThenBy(bid => bid.Id).Select(bid => bid.Id),
            first.Items.Concat(second.Items).Select(bid => bid.Id));
        Assert.Empty(db.ChangeTracker.Entries());
    }

    [PostgresFact]
    public async Task LastBidSort_PreservesPostgreSqlNullableOrderingInBothDirections()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        await using var db = Open(database);
        await db.Database.MigrateAsync();
        var userId = Guid.NewGuid();
        var neverBid = AutoBid.Create(Guid.NewGuid(), userId, "buyer", 100m);
        var hasBid = AutoBid.Create(Guid.NewGuid(), userId, "buyer", 100m);
        hasBid.RecordBid(10m);
        db.AutoBids.AddRange(neverBid, hasBid);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var repository = new AutoBidRepository(db, null!, null!);
        var ascending = await repository.GetAutoBidsByUserAsync(userId,
            new AutoBidQueryParams { SortBy = "lastBidAt", SortDescending = false });
        var descending = await repository.GetAutoBidsByUserAsync(userId,
            new AutoBidQueryParams { SortBy = "lastBidAt", SortDescending = true });

        Assert.Equal(new[] { hasBid.Id, neverBid.Id }, ascending.Items.Select(bid => bid.Id));
        Assert.Equal(new[] { neverBid.Id, hasBid.Id }, descending.Items.Select(bid => bid.Id));
        Assert.Empty(db.ChangeTracker.Entries());
    }

    private static BidDbContext Open(PostgresTestDatabase database) => new(new DbContextOptionsBuilder<BidDbContext>()
        .UseNpgsql(database.ConnectionString).Options);
}
