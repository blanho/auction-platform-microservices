using Auctions.Domain.Entities;
using Auctions.Domain.Enums;
using Auctions.Infrastructure.Persistence;
using BuildingBlocks.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using TestSupport;
using Xunit;
using AuctionEntity = Auctions.Domain.Entities.Auction;

namespace Auction.Infrastructure.Tests;

public class BuyNowReservationPersistenceTests
{
    [PostgresFact]
    public async Task CancellationFenceAndReservationOwnershipSurviveReload()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        var options = new DbContextOptionsBuilder<AuctionDbContext>().UseNpgsql(database.ConnectionString).Options;
        var auction = AuctionEntity.Create(new CreateAuctionParams(Guid.NewGuid(), "seller", Item.Create("item", "description"),
            10, DateTimeOffset.UtcNow.AddHours(1), BuyNowPrice: 100));
        var cancelled = Guid.NewGuid(); var owner = Guid.NewGuid(); var buyer = Guid.NewGuid();
        await using (var db = new AuctionDbContext(options))
        {
            await db.Database.MigrateAsync();
            auction.ReleaseBuyNow(cancelled);
            auction.ReserveBuyNow(owner, buyer);
            db.Auctions.Add(auction);
            await db.SaveChangesAsync();
        }
        await using (var db = new AuctionDbContext(options))
        {
            var persisted = await db.Auctions.Include(x => x.Item).SingleAsync();
            Assert.Contains(cancelled, persisted.CancelledBuyNowAttempts);
            Assert.Throws<DomainInvariantException>(() => persisted.ReserveBuyNow(cancelled, Guid.NewGuid()));
            persisted.ReleaseBuyNow(cancelled);
            Assert.Equal(owner, persisted.BuyNowCorrelationId);
            Assert.Equal(buyer, persisted.BuyNowBuyerId);
            persisted.CompleteReservedBuyNow(owner, buyer, "buyer", Guid.NewGuid());
            await db.SaveChangesAsync();
        }
        await using var check = new AuctionDbContext(options);
        var completed = await check.Auctions.SingleAsync();
        Assert.Equal(Status.Finished, completed.Status);
        Assert.NotNull(completed.BuyNowOrderId);
    }

    [PostgresFact]
    public async Task ConcurrentReleaseAndCompletionCannotBothCommit()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        var options = new DbContextOptionsBuilder<AuctionDbContext>().UseNpgsql(database.ConnectionString).Options;
        var correlation = Guid.NewGuid(); var buyer = Guid.NewGuid();
        await using (var db = new AuctionDbContext(options))
        {
            await db.Database.MigrateAsync();
            var auction = AuctionEntity.Create(new CreateAuctionParams(Guid.NewGuid(), "seller", Item.Create("item", "description"),
                10, DateTimeOffset.UtcNow.AddHours(1), BuyNowPrice: 100));
            auction.ReserveBuyNow(correlation, buyer); db.Add(auction); await db.SaveChangesAsync();
        }
        await using var completingDb = new AuctionDbContext(options);
        await using var releasingDb = new AuctionDbContext(options);
        var completing = await completingDb.Auctions.Include(x => x.Item).SingleAsync();
        var releasing = await releasingDb.Auctions.Include(x => x.Item).SingleAsync();
        completing.CompleteReservedBuyNow(correlation, buyer, "buyer", Guid.NewGuid());
        releasing.ReleaseBuyNow(correlation);
        await completingDb.SaveChangesAsync();
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => releasingDb.SaveChangesAsync());
    }
}
