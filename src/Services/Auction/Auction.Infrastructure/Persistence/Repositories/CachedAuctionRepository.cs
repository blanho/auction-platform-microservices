#nullable enable
using Auctions.Application.DTOs;
using Auctions.Domain.Entities;
using AutoMapper;
using BuildingBlocks.Application.Constants;
using BuildingBlocks.Domain.Enums;
using Microsoft.Extensions.Logging;
using BuildingBlocks.Infrastructure.Caching;
using BuildingBlocks.Infrastructure.Repository;
using BuildingBlocks.Infrastructure.Repository.Specifications;

namespace Auctions.Infrastructure.Persistence.Repositories;

public class CachedAuctionRepository : IAuctionRepository
{
    private readonly IAuctionRepository _inner;
    private readonly ICacheService _cache;
    private readonly IMapper _mapper;
    private readonly ILogger<CachedAuctionRepository> _logger;
    private static readonly TimeSpan SingleAuctionTtl = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan AuctionListTtl = TimeSpan.FromMinutes(1);

    public CachedAuctionRepository(IAuctionRepository inner, ICacheService cache, IMapper mapper, ILogger<CachedAuctionRepository> logger)
    {
        _inner = inner;
        _cache = cache;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<List<Auction>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var key = CacheKeys.AuctionList();
        var cachedDtos = await _cache.GetAsync<List<Application.DTOs.AuctionDto>>(key, cancellationToken);
        if (cachedDtos != null)
        {
            _logger.LogInformation("Cache HIT for all auctions");
            return _mapper.Map<List<Auction>>(cachedDtos);
        }

        _logger.LogInformation("Cache MISS for all auctions - fetching from database");
        var auctions = await _inner.GetAllAsync(cancellationToken);
        var dtos = _mapper.Map<List<Application.DTOs.AuctionDto>>(auctions);
        await _cache.SetAsync(key, dtos, AuctionListTtl, cancellationToken);
        return auctions;
    }

    public async Task<Auction> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var key = CacheKeys.Auction(id);
        var cachedDto = await _cache.GetAsync<Application.DTOs.AuctionDto>(key, cancellationToken);
        if (cachedDto != null)
        {
            _logger.LogInformation("Cache HIT for auction {AuctionId}", id);
            return _mapper.Map<Auction>(cachedDto);
        }

        _logger.LogInformation("Cache MISS for auction {AuctionId} - fetching from database", id);
        var auction = await _inner.GetByIdAsync(id, cancellationToken);
        if (auction != null)
        {
            var dto = _mapper.Map<Application.DTOs.AuctionDto>(auction);
            await _cache.SetAsync(key, dto, SingleAuctionTtl, cancellationToken);
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

    public Task<(List<Auction> Items, int TotalCount)> GetPagedAsync(
        string? status = null,
        string? seller = null,
        string? winner = null,
        string? searchTerm = null,
        string? category = null,
        bool? isFeatured = null,
        string? orderBy = null,
        bool descending = true,
        int pageNumber = PaginationDefaults.DefaultPage,
        int pageSize = PaginationDefaults.DefaultPageSize,
        CancellationToken cancellationToken = default)
        => _inner.GetPagedAsync(status, seller, winner, searchTerm, category, isFeatured, orderBy, descending, pageNumber, pageSize, cancellationToken);

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

    private async Task InvalidateAfterWrite(Guid id, CancellationToken cancellationToken)
    {
        await _cache.RemoveAsync(CacheKeys.Auction(id), cancellationToken);
        await _cache.RemoveAsync(CacheKeys.AuctionList(), cancellationToken);
    }
}

