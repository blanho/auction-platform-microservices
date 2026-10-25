using System.Security.Cryptography;
using System.Text.Json;

namespace Auctions.Application.Services;

public sealed class CachedAuctionBrowseService(
    IPaginatedAuctionQueryService inner,
    ICacheService cache,
    ILogger<CachedAuctionBrowseService> logger) : IPaginatedAuctionQueryService
{
    private static readonly SemaphoreSlim[] Gates = Enumerable.Range(0, 64)
        .Select(_ => new SemaphoreSlim(1, 1)).ToArray();
    private static readonly TimeSpan Ttl = TimeSpan.FromSeconds(5);

    public async Task<PaginatedResult<AuctionDto>> GetPagedAuctionsAsync(
        AuctionFilterDto queryParams, CancellationToken cancellationToken)
    {
        if (!string.Equals(queryParams.Filter?.Status, "Live", StringComparison.OrdinalIgnoreCase)
            || !string.IsNullOrEmpty(queryParams.Filter?.Seller)
            || !string.IsNullOrEmpty(queryParams.Filter?.Winner))
            return await inner.GetPagedAuctionsAsync(queryParams, cancellationToken);

        var hash = SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(queryParams));
        var key = $"auction:browse:dto:v1:{Convert.ToHexString(hash)}";
        var gate = Gates[hash[0] % Gates.Length];
        if (!await gate.WaitAsync(TimeSpan.FromMilliseconds(100), cancellationToken))
            return await inner.GetPagedAuctionsAsync(queryParams, cancellationToken);
        try
        {
            try
            {
                var hit = await cache.GetAsync<PaginatedResult<AuctionDto>>(key, cancellationToken)
                    .WaitAsync(TimeSpan.FromMilliseconds(200), cancellationToken);
                if (hit is not null) return hit;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogWarning(ex, "Browse cache unavailable; reading auctions from PostgreSQL");
                return await inner.GetPagedAuctionsAsync(queryParams, cancellationToken);
            }

            var result = await inner.GetPagedAuctionsAsync(queryParams, cancellationToken);
            try
            {
                await cache.SetAsync(key, result, Ttl, cancellationToken)
                    .WaitAsync(TimeSpan.FromMilliseconds(200), cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogWarning(ex, "Could not populate auction browse cache");
            }
            return result;
        }
        finally
        {
            gate.Release();
        }
    }
}
