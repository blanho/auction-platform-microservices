using System.Security.Cryptography;
using System.Text.Json;
using Auctions.Domain.Enums;

namespace Auctions.Application.Services;

public sealed class CachedAuctionBrowseService(
    IPaginatedAuctionQueryService inner,
    ICacheService cache,
    ILogger<CachedAuctionBrowseService> logger) : IPaginatedAuctionQueryService
{
    private const int GateCount = 64;
    private const string CacheKeyPrefix = "auction:browse:dto:v1:";

    private static readonly TimeSpan CacheLifetime = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan GateWaitTimeout = TimeSpan.FromMilliseconds(100);
    private static readonly TimeSpan CacheOperationTimeout = TimeSpan.FromMilliseconds(200);
    private static readonly SemaphoreSlim[] Gates = Enumerable.Range(0, GateCount)
        .Select(_ => new SemaphoreSlim(1, 1)).ToArray();

    public async Task<PaginatedResult<AuctionDto>> GetPagedAuctionsAsync(
        AuctionFilterDto queryParams,
        CancellationToken cancellationToken)
    {
        if (!IsCacheable(queryParams.Filter))
        {
            return await inner.GetPagedAuctionsAsync(queryParams, cancellationToken);
        }

        var hash = SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(queryParams));
        var cacheKey = $"{CacheKeyPrefix}{Convert.ToHexString(hash)}";
        var gate = Gates[hash[0] % Gates.Length];

        if (!await gate.WaitAsync(GateWaitTimeout, cancellationToken))
        {
            return await inner.GetPagedAuctionsAsync(queryParams, cancellationToken);
        }

        try
        {
            return await GetOrLoadAsync(cacheKey, queryParams, cancellationToken);
        }
        finally
        {
            gate.Release();
        }
    }

    private static bool IsCacheable(AuctionFilter? filter) =>
        string.Equals(filter?.Status, nameof(Status.Live), StringComparison.OrdinalIgnoreCase)
        && string.IsNullOrEmpty(filter?.Seller)
        && string.IsNullOrEmpty(filter?.Winner);

    private async Task<PaginatedResult<AuctionDto>> GetOrLoadAsync(
        string cacheKey,
        AuctionFilterDto queryParams,
        CancellationToken cancellationToken)
    {
        try
        {
            var cachedResult = await cache.GetAsync<PaginatedResult<AuctionDto>>(cacheKey, cancellationToken)
                .WaitAsync(CacheOperationTimeout, cancellationToken);

            if (cachedResult is not null)
            {
                return cachedResult;
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Browse cache unavailable; reading auctions from PostgreSQL");
            return await inner.GetPagedAuctionsAsync(queryParams, cancellationToken);
        }

        var result = await inner.GetPagedAuctionsAsync(queryParams, cancellationToken);

        try
        {
            await cache.SetAsync(cacheKey, result, CacheLifetime, cancellationToken)
                .WaitAsync(CacheOperationTimeout, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Could not populate auction browse cache");
        }

        return result;
    }
}
