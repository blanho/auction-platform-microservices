using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Infrastructure.Caching;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Xunit;

namespace BuildingBlocks.Infrastructure.Tests;

public sealed class RedisCacheServiceTests
{
    [Fact]
    public async Task GetOrSetAsync_DistinguishesAMissingValueTypeFromACachedDefault()
    {
        var storage = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
        ICacheService cache = new RedisCacheService(storage);

        var first = await cache.GetOrSetAsync("counter", _ => Task.FromResult(7));
        Assert.Equal(7, first);
        Assert.Equal(7, await cache.GetAsync<int>("counter"));

        await cache.SetAsync("counter", 0);

        var second = await cache.GetOrSetAsync<int>("counter",
            _ => throw new InvalidOperationException("Factory should not run on a cache hit."));
        Assert.Equal(0, second);
    }
}
