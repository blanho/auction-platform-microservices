using System.Reflection;
using Auctions.Domain.Entities;
using Auctions.Domain.Enums;
using Auctions.Infrastructure.Persistence;
using Auctions.Infrastructure.Persistence.Repositories;
using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Application.Abstractions.Providers;
using Microsoft.EntityFrameworkCore;
using Xunit;
using AuctionEntity = Auctions.Domain.Entities.Auction;

namespace Auction.Infrastructure.Tests;

public class AuctionRepositoryTrackingTests
{
    [Fact]
    public async Task UpdatingTrackedAuction_DoesNotMarkItsItemModified()
    {
        var options = new DbContextOptionsBuilder<AuctionDbContext>()
            .UseNpgsql("Host=localhost;Database=auction_tracking_test")
            .Options;
        await using var context = new AuctionDbContext(options);
        var auction = AuctionEntity.CreateScheduled(Guid.NewGuid(), "seller",
            Item.Create("Item", "Description"), 1m, DateTimeOffset.UtcNow.AddDays(1));
        context.Auctions.Attach(auction);
        var auditContext = Stub<IAuditContext>((method, _) => method.Name switch
        {
            "get_UserId" => Guid.Empty,
            _ => throw new NotSupportedException(method.Name)
        });
        var repository = new AuctionRepository(context, new DateTimeProvider(), auditContext);

        auction.ChangeStatus(Status.Live);
        await repository.UpdateAsync(auction);
        context.ChangeTracker.DetectChanges();

        Assert.Equal(EntityState.Modified, context.Entry(auction).State);
        Assert.True(context.Entry(auction).Property(a => a.Status).IsModified);
        Assert.Equal(EntityState.Unchanged, context.Entry(auction.Item).State);
    }

    private static T Stub<T>(Func<MethodInfo, object?[]?, object?> handler) where T : class
    {
        var proxy = DispatchProxy.Create<T, TestProxy>();
        ((TestProxy)(object)proxy).Handler = handler;
        return proxy;
    }

    [Fact]
    public async Task CreatingAuction_UsesOneAuditTimestampForRootAndItem()
    {
        await using var context = new AuctionDbContext(new DbContextOptionsBuilder<AuctionDbContext>()
            .UseNpgsql("Host=localhost;Database=auction_tracking_test").Options);
        var actor = Guid.NewGuid();
        var audit = Stub<IAuditContext>((_, _) => actor);
        var repository = new AuctionRepository(context, new DateTimeProvider(), audit);
        var auction = AuctionEntity.CreateScheduled(Guid.NewGuid(), "seller",
            Item.Create("Item", "Description"), 1m, DateTimeOffset.UtcNow.AddDays(1));

        await repository.CreateAsync(auction);

        Assert.Equal(actor, auction.CreatedBy);
        Assert.Equal(actor, auction.Item.CreatedBy);
        Assert.Equal(auction.CreatedAt, auction.Item.CreatedAt);
        Assert.Equal(EntityState.Added, context.Entry(auction).State);
        Assert.Equal(EntityState.Added, context.Entry(auction.Item).State);
    }

    [Fact]
    public async Task DetachedAuctionUpdate_PreservesExplicitGraphUpdateBehavior()
    {
        await using var context = new AuctionDbContext(new DbContextOptionsBuilder<AuctionDbContext>()
            .UseNpgsql("Host=localhost;Database=auction_tracking_test").Options);
        var audit = Stub<IAuditContext>((_, _) => Guid.Empty);
        var repository = new AuctionRepository(context, new DateTimeProvider(), audit);
        var auction = AuctionEntity.CreateScheduled(Guid.NewGuid(), "seller",
            Item.Create("Item", "Description"), 1m, DateTimeOffset.UtcNow.AddDays(1));
        auction.Item.UpdateTitle("Changed title");

        await repository.UpdateAsync(auction);

        Assert.Equal(EntityState.Modified, context.Entry(auction).State);
        Assert.Equal(EntityState.Modified, context.Entry(auction.Item).State);
    }

    [Fact]
    public async Task BookmarkWrites_KeepOwnerAsAuditActor()
    {
        await using var context = new AuctionDbContext(new DbContextOptionsBuilder<AuctionDbContext>()
            .UseNpgsql("Host=localhost;Database=auction_tracking_test").Options);
        var owner = Guid.NewGuid();
        var audit = Stub<IAuditContext>((_, _) => Guid.NewGuid());
        var repository = new BookmarkRepository(context, new DateTimeProvider(), audit);
        var bookmark = Bookmark.Create(owner, "owner", Guid.NewGuid(), BookmarkType.Watchlist);

        await repository.AddAsync(bookmark);
        await repository.UpdateAsync(bookmark);
        await repository.DeleteAsync(bookmark);

        Assert.Equal(owner, bookmark.CreatedBy);
        Assert.Equal(owner, bookmark.UpdatedBy);
        Assert.Equal(owner, bookmark.DeletedBy);
        Assert.True(bookmark.IsDeleted);
    }

    public class TestProxy : DispatchProxy
    {
        public Func<MethodInfo, object?[]?, object?> Handler { get; set; } = null!;
        protected override object? Invoke(MethodInfo? method, object?[]? args) => Handler(method!, args);
    }
}
