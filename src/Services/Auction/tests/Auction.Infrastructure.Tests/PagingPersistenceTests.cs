using Auctions.Application.DTOs;
using Auctions.Application.Filtering;
using Auctions.Domain.Entities;
using Auctions.Infrastructure.Persistence;
using Auctions.Infrastructure.Persistence.Repositories;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Application.Abstractions.Providers;
using BuildingBlocks.Application.Paging;
using BuildingBlocks.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using TestSupport;
using Xunit;
using AuctionEntity = Auctions.Domain.Entities.Auction;

namespace Auction.Infrastructure.Tests;

public sealed class PagingPersistenceTests
{
    [PostgresFact]
    public async Task SharedEfPaging_NormalizesMetadataProjectsInSqlAndDoesNotOverflow()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        await using var db = Open(database);
        await SeedAsync(db);
        var query = db.Auctions.AsNoTracking().OrderBy(auction => auction.Id);

        var all = await query.ToPaginatedResultAsync(0, 0);
        Assert.Equal(1, all.Page);
        Assert.Equal(10, all.PageSize);
        Assert.Equal(3, all.TotalCount);
        Assert.Equal(3, all.Items.Count);
        var projected = await query.ToPaginatedResultAsync(2, 1, auction => auction.Item.Title);
        Assert.Equal(3, projected.TotalCount);
        Assert.Single(projected.Items);
        Assert.Equal((await query.Select(auction => auction.Item.Title).ToListAsync())[1], projected.Items[0]);
        var extreme = await query.ToPaginatedResultAsync(int.MaxValue, 100);
        Assert.Empty(extreme.Items);
        Assert.Equal(21474837, extreme.Page);
        Assert.Empty(db.ChangeTracker.Entries());
        using var cancelled = new CancellationTokenSource();
        await cancelled.CancelAsync();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => query.ToPaginatedResultAsync(1, 10, cancelled.Token));
    }

    [PostgresFact]
    public async Task UnknownMultiSort_UsesStableDefaultAcrossDatabasePages()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        await using var db = Open(database);
        await SeedAsync(db);
        var repository = Repository(db);
        var actual = new List<Guid>();
        for (var page = 1; page <= 3; page++)
        {
            var result = await repository.GetPagedAsync(new AuctionFilterDto
            {
                Page = page, PageSize = 1,
                Sorts = [new SortDescriptor { Field = "unknown" }]
            });
            Assert.Equal(3, result.TotalCount);
            actual.Add(Assert.Single(result.Items).Id);
        }
        Assert.Equal(await db.Auctions.OrderBy(auction => auction.Id).Select(auction => auction.Id).ToListAsync(), actual);
    }

    [PostgresFact]
    public async Task AuctionAndBookmarkSortAliases_KeepComputedPriceAndEndDateOrdering()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        await using var db = Open(database);
        await db.Database.MigrateAsync();
        var timestamp = DateTimeOffset.UtcNow;
        var auctions = Enumerable.Range(1, 3).Select(index => AuctionEntity.CreateScheduled(Guid.NewGuid(), "seller",
            Item.Create($"Item {index}", "description"), index * 10m, timestamp.AddDays(4 - index))).ToArray();
        auctions[0].UpdateHighBid(100m);
        auctions[2].UpdateHighBid(30m);
        db.Auctions.AddRange(auctions);
        var bookmarks = auctions.Select(auction => Bookmark.Create(Guid.NewGuid(), "user", auction.Id, BookmarkType.Watchlist)).ToArray();
        db.Bookmarks.AddRange(bookmarks);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var repository = Repository(db);
        var byPrice = await repository.GetPagedAsync(new AuctionFilterDto { SortBy = "PRICE", SortDescending = false });
        var byEndDate = await repository.GetPagedAsync(new AuctionFilterDto { SortBy = "enddate", SortDescending = false });

        Assert.Equal(new[] { auctions[1].Id, auctions[2].Id, auctions[0].Id }, byPrice.Items.Select(auction => auction.Id));
        Assert.Equal(auctions.Reverse().Select(auction => auction.Id), byEndDate.Items.Select(auction => auction.Id));

        var bookmarkRepository = new BookmarkRepository(db, new DateTimeProvider(), null!);
        var byAuctionEnd = await bookmarkRepository.GetByUsernameAsync("user", BookmarkType.Watchlist,
            new BookmarkQueryParams { SortBy = "auctionEnd", SortDescending = false });
        var byCurrentBid = await bookmarkRepository.GetByUsernameAsync("user", BookmarkType.Watchlist,
            new BookmarkQueryParams { SortBy = "currentBid", SortDescending = false });

        Assert.Equal(bookmarks.Reverse().Select(bookmark => bookmark.Id), byAuctionEnd.Items.Select(bookmark => bookmark.Id));
        Assert.Equal(new[] { bookmarks[1].Id, bookmarks[2].Id, bookmarks[0].Id }, byCurrentBid.Items.Select(bookmark => bookmark.Id));
        Assert.Empty(db.ChangeTracker.Entries());
    }

    [PostgresFact]
    public async Task AuctionCache_IsolatesMultiSortsAndReusesNormalizedPageKeys()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        await using var db = Open(database);
        await SeedAsync(db);
        var cache = new DictionaryCache();
        var repository = new CachedAuctionRepository(Repository(db), cache, NullLogger<CachedAuctionRepository>.Instance);
        AuctionFilterDto Filter(bool descending) => new()
        {
            PageSize = 1,
            Sorts =
            [
                new SortDescriptor { Field = "price" },
                new SortDescriptor { Field = "title", Desc = descending }
            ]
        };

        var first = await repository.GetPagedAsync(Filter(false));
        var second = await repository.GetPagedAsync(Filter(true));
        Assert.Equal("Alpha", Assert.Single(first.Items).Item.Title);
        Assert.Equal("Zulu", Assert.Single(second.Items).Item.Title);
        Assert.Equal(first, await repository.GetPagedAsync(Filter(false)));
        Assert.Equal(2, cache.Values.Count);

        var normalized = await repository.GetPagedAsync(0, 0);
        var repeated = await repository.GetPagedAsync(1, 10);
        Assert.Equal(normalized, repeated);
        Assert.Equal(3, cache.Values.Count);
        Assert.Equal(1, repeated.Page);
        Assert.Equal(10, repeated.PageSize);
    }

    private static AuctionDbContext Open(PostgresTestDatabase database) => new(new DbContextOptionsBuilder<AuctionDbContext>()
        .UseNpgsql(database.ConnectionString).Options);

    private static AuctionRepository Repository(AuctionDbContext db) => new(db, new DateTimeProvider(),
        TestProxy.Create<IAuditContext>((method, _) => method.Name == "get_UserId" ? Guid.Empty : throw new NotSupportedException(method.Name)));

    private static async Task SeedAsync(AuctionDbContext db)
    {
        await db.Database.MigrateAsync();
        var timestamp = DateTimeOffset.UtcNow.AddDays(-1);
        foreach (var title in new[] { "Zulu", "Alpha", "Middle" })
        {
            var auction = AuctionEntity.CreateScheduled(Guid.NewGuid(), "seller", Item.Create(title, "description"),
                10m, DateTimeOffset.UtcNow.AddDays(1));
            auction.SetCreatedAudit(Guid.Empty, timestamp);
            db.Auctions.Add(auction);
        }
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
    }

    private sealed class DictionaryCache : ICacheService
    {
        public Dictionary<string, object> Values { get; } = [];
        public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) =>
            Task.FromResult(Values.TryGetValue(key, out var value) ? (T)value : default);
        public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        {
            Values[key] = value!;
            return Task.CompletedTask;
        }
        public Task RemoveAsync(string key, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
