using Auctions.Application.DTOs;
using Auctions.Application.Interfaces;
using Auctions.Application.Services;
using BuildingBlocks.Application.Abstractions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Auction.Application.Tests;

public class CachedAuctionBrowseTests
{
    [Fact]
    public async Task ConcurrentLiveBrowseRequestsShareOneDatabaseReadAndCacheDtos()
    {
        var inner = new Query();
        var cache = new Cache();
        var service = Service(inner, cache);
        await Task.WhenAll(Enumerable.Range(0, 10).Select(_ => service.GetPagedAuctionsAsync(Live(), default)));
        Assert.Equal(1, inner.Calls);
        Assert.Equal(TimeSpan.FromSeconds(5), cache.Ttl);
        Assert.IsType<PaginatedResult<AuctionDto>>(Assert.Single(cache.Values).Value);
    }

    [Fact]
    public async Task DifferentFiltersHaveSeparateKeys()
    {
        var inner = new Query();
        var service = Service(inner, new Cache());
        await service.GetPagedAuctionsAsync(Live(), default);
        await service.GetPagedAuctionsAsync(new() { Page = 2, Filter = new() { Status = "Live" } }, default);
        Assert.Equal(2, inner.Calls);
    }

    [Theory]
    [InlineData(null, null, null)]
    [InlineData("Draft", null, null)]
    [InlineData("Live", "seller", null)]
    [InlineData("Live", null, "winner")]
    public async Task NonPublicOrPersonalQueriesBypassCache(string? status, string? seller, string? winner)
    {
        var inner = new Query();
        var cache = new Cache();
        var service = Service(inner, cache);
        var filter = new AuctionFilterDto { Filter = new() { Status = status, Seller = seller, Winner = winner } };
        await service.GetPagedAuctionsAsync(filter, default);
        await service.GetPagedAuctionsAsync(filter, default);
        Assert.Equal(2, inner.Calls);
        Assert.Empty(cache.Values);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task CacheOutageDoesNotFailBrowseOrRepeatDatabaseQuery(bool failRead, bool failWrite)
    {
        var inner = new Query();
        var result = await Service(inner, new Cache { FailRead = failRead, FailWrite = failWrite })
            .GetPagedAuctionsAsync(Live(), default);
        Assert.NotNull(result);
        Assert.Equal(1, inner.Calls);
    }

    [Fact]
    public async Task StalledCacheFallsBackWithoutWaitingForItsConnectionTimeout()
    {
        var inner = new Query();
        var cache = new Cache { StallRead = true };
        var request = Service(inner, cache).GetPagedAuctionsAsync(Live(), default);
        var result = await request.WaitAsync(TimeSpan.FromSeconds(3));
        Assert.NotNull(result);
        Assert.Equal(1, inner.Calls);
    }

    [Fact]
    public async Task CancellationIsNotTreatedAsCacheFailure()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        var inner = new Query();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Service(inner, new Cache())
            .GetPagedAuctionsAsync(Live(), cts.Token));
        Assert.Equal(0, inner.Calls);
    }

    private static AuctionFilterDto Live() => new() { Filter = new() { Status = "Live" } };
    private static CachedAuctionBrowseService Service(Query inner, Cache cache) =>
        new(inner, cache, NullLogger<CachedAuctionBrowseService>.Instance);

    private sealed class Query : IPaginatedAuctionQueryService
    {
        public int Calls;
        public async Task<PaginatedResult<AuctionDto>> GetPagedAuctionsAsync(AuctionFilterDto queryParams, CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref Calls);
            await Task.Delay(10, cancellationToken);
            return PaginatedResult<AuctionDto>.Empty(queryParams.Page, queryParams.PageSize);
        }
    }

    private sealed class Cache : ICacheService
    {
        public readonly Dictionary<string, object> Values = new();
        public TimeSpan? Ttl;
        public bool FailRead;
        public bool FailWrite;
        public bool StallRead;
        public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            if (FailRead) throw new InvalidOperationException("Cache offline");
            if (StallRead) return new TaskCompletionSource<T?>().Task;
            return Task.FromResult(Values.TryGetValue(key, out var value) ? (T)value : default);
        }
        public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        {
            if (FailWrite) throw new InvalidOperationException("Cache offline");
            Values[key] = value!;
            Ttl = expiration;
            return Task.CompletedTask;
        }
        public Task RemoveAsync(string key, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
