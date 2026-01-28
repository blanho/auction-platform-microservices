#nullable enable
using Auctions.Application.DTOs;
using Auctions.Domain.Entities;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Constants;
using BuildingBlocks.Domain.Enums;
using Microsoft.Extensions.Logging;
using BuildingBlocks.Infrastructure.Caching;
using BuildingBlocks.Infrastructure.Repository;

namespace Auctions.Infrastructure.Persistence.Repositories;

public class CachedAuctionRepository : IAuctionRepository
{
    private readonly IAuctionRepository _inner;
    private readonly ICacheService _cache;
    private readonly ILogger<CachedAuctionRepository> _logger;
    private static readonly TimeSpan SingleAuctionTtl = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan AuctionListTtl = TimeSpan.FromMinutes(1);

    public CachedAuctionRepository(IAuctionRepository inner, ICacheService cache, ILogger<CachedAuctionRepository> logger)
    {
        _inner = inner;
        _cache = cache;
        _logger = logger;
    }

    public async Task<PaginatedResult<Auction>> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var key = CacheKeys.AuctionList($"page:{page}:size:{pageSize}");
        var cached = await _cache.GetAsync<PaginatedResult<Auction>>(key, cancellationToken);
        if (cached != null)
        {
            _logger.LogInformation("Cache HIT for paged auctions (page {Page}, size {Size})", page, pageSize);
            return cached;
        }

        _logger.LogInformation("Cache MISS for paged auctions - fetching from database");
        var result = await _inner.GetPagedAsync(page, pageSize, cancellationToken);
        await _cache.SetAsync(key, result, AuctionListTtl, cancellationToken);
        return result;
    }

    public async Task<PaginatedResult<Auction>> GetPagedAsync(
        AuctionFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        var key = CacheKeys.AuctionList($"page:{filter.PageNumber}:size:{filter.PageSize}:status:{filter.Status}:seller:{filter.Seller}");
        var cached = await _cache.GetAsync<PaginatedResult<Auction>>(key, cancellationToken);
        if (cached != null)
        {
            _logger.LogInformation("Cache HIT for paged auctions (page {Page}, size {Size})", filter.PageNumber, filter.PageSize);
            return cached;
        }

        _logger.LogInformation("Cache MISS for paged auctions - fetching from database");
        var result = await _inner.GetPagedAsync(filter, cancellationToken);
        await _cache.SetAsync(key, result, AuctionListTtl, cancellationToken);
        return result;
    }

    public async Task<Auction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var key = CacheKeys.Auction(id);
        var cachedAuction = await _cache.GetAsync<Auction>(key, cancellationToken);
        if (cachedAuction != null)
        {
            _logger.LogInformation("Cache HIT for auction {AuctionId}", id);
            return cachedAuction;
        }

        _logger.LogInformation("Cache MISS for auction {AuctionId} - fetching from database", id);
        var auction = await _inner.GetByIdAsync(id, cancellationToken);
        if (auction != null)
        {
            await _cache.SetAsync(key, auction, SingleAuctionTtl, cancellationToken);
            _logger.LogInformation("Cache SET for auction {AuctionId}", id);
        }
        return auction ?? throw new KeyNotFoundException($"Auction with ID {id} not found");
    }

    public async Task<Auction> CreateAsync(Auction auction, CancellationToken cancellationToken = default)
    {
        var result = await _inner.CreateAsync(auction, cancellationToken);
        await InvalidateAfterWrite(result.Id, cancellationToken);
        return result;
    }

    public async Task UpdateAsync(Auction auction, CancellationToken cancellationToken = default)
    {
        await _inner.UpdateAsync(auction, cancellationToken);
        await InvalidateAfterWrite(auction.Id, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _inner.DeleteAsync(id, cancellationToken);
        await InvalidateAfterWrite(id, cancellationToken);
    }

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        => _inner.ExistsAsync(id, cancellationToken);

    public Task<List<Auction>> GetFinishedAuctionsAsync(CancellationToken cancellationToken = default)
        => _inner.GetFinishedAuctionsAsync(cancellationToken);

    public Task<List<Auction>> GetAuctionsToAutoDeactivateAsync(CancellationToken cancellationToken = default)
        => _inner.GetAuctionsToAutoDeactivateAsync(cancellationToken);

    public Task<List<Auction>> GetScheduledAuctionsToActivateAsync(CancellationToken cancellationToken = default)
        => _inner.GetScheduledAuctionsToActivateAsync(cancellationToken);

    public Task<List<Auction>> GetAuctionsEndingBetweenAsync(
        DateTime startTime,
        DateTime endTime,
        CancellationToken cancellationToken = default)
        => _inner.GetAuctionsEndingBetweenAsync(startTime, endTime, cancellationToken);

    public Task<List<Auction>> GetAuctionsForExportAsync(
        Status? status = null,
        string? seller = null,
        DateTimeOffset? startDate = null,
        DateTimeOffset? endDate = null,
        CancellationToken cancellationToken = default)
        => _inner.GetAuctionsForExportAsync(status, seller, startDate, endDate, cancellationToken);

    public Task<int> CountLiveAuctionsAsync(CancellationToken cancellationToken = default)
        => _inner.CountLiveAuctionsAsync(cancellationToken);

    public Task<int> CountEndingSoonAsync(CancellationToken cancellationToken = default)
        => _inner.CountEndingSoonAsync(cancellationToken);

    public Task<int> GetCountByStatusAsync(Status status, CancellationToken cancellationToken = default)
        => _inner.GetCountByStatusAsync(status, cancellationToken);

    public Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
        => _inner.GetTotalCountAsync(cancellationToken);

    public Task<decimal> GetTotalRevenueAsync(CancellationToken cancellationToken = default)
        => _inner.GetTotalRevenueAsync(cancellationToken);

    public Task<List<Auction>> GetTrendingItemsAsync(int limit, CancellationToken cancellationToken = default)
        => _inner.GetTrendingItemsAsync(limit, cancellationToken);

    public Task<List<Auction>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
        => _inner.GetByIdsAsync(ids, cancellationToken);

    public Task<int> GetCountEndingBetweenAsync(DateTimeOffset start, DateTimeOffset end, CancellationToken cancellationToken = default)
        => _inner.GetCountEndingBetweenAsync(start, end, cancellationToken);

    public Task<List<Auction>> GetTopByRevenueAsync(int limit, CancellationToken cancellationToken = default)
        => _inner.GetTopByRevenueAsync(limit, cancellationToken);

    public Task<List<CategoryStatDto>> GetCategoryStatsAsync(CancellationToken cancellationToken = default)
        => _inner.GetCategoryStatsAsync(cancellationToken);

    public Task<List<Auction>> GetBySellerUsernameAsync(string username, CancellationToken cancellationToken = default)
        => _inner.GetBySellerUsernameAsync(username, cancellationToken);

    public Task<List<Auction>> GetWonByUsernameAsync(string username, CancellationToken cancellationToken = default)
        => _inner.GetWonByUsernameAsync(username, cancellationToken);

    public Task<SellerStatsDto> GetSellerStatsAsync(
        string username, 
        DateTimeOffset periodStart, 
        DateTimeOffset? previousPeriodStart = null, 
        CancellationToken cancellationToken = default)
        => _inner.GetSellerStatsAsync(username, periodStart, previousPeriodStart, cancellationToken);

    public Task<List<Auction>> GetActiveAuctionsBySellerIdAsync(Guid sellerId, CancellationToken cancellationToken = default)
        => _inner.GetActiveAuctionsBySellerIdAsync(sellerId, cancellationToken);

    public Task<List<Auction>> GetAllBySellerIdAsync(Guid sellerId, CancellationToken cancellationToken = default)
        => _inner.GetAllBySellerIdAsync(sellerId, cancellationToken);

    public Task<List<Auction>> GetAuctionsWithWinnerIdAsync(Guid winnerId, CancellationToken cancellationToken = default)
        => _inner.GetAuctionsWithWinnerIdAsync(winnerId, cancellationToken);

    public Task<int> GetWatchlistCountByUsernameAsync(string username, CancellationToken cancellationToken = default)
        => _inner.GetWatchlistCountByUsernameAsync(username, cancellationToken);

    private async Task InvalidateAfterWrite(Guid id, CancellationToken cancellationToken)
    {
        await _cache.RemoveAsync(CacheKeys.Auction(id), cancellationToken);
        await _cache.RemoveAsync(CacheKeys.AuctionList(), cancellationToken);
    }
}

