#nullable enable
using AuctionService.Application.Interfaces;
using AuctionService.Domain.Entities;
using AutoMapper;
using Common.Caching.Abstractions;
using Common.Caching.Keys;
using Common.Domain.Enums;
using Common.Repository.Interfaces;

namespace AuctionService.Infrastructure.Repositories;

public class CachedAuctionRepository : IAuctionRepository
{
    private readonly IAuctionRepository _inner;
    private readonly ICacheService _cache;
    private readonly IMapper _mapper;
    private readonly IAppLogger<CachedAuctionRepository> _logger;
    private static readonly TimeSpan SingleAuctionTtl = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan AuctionListTtl = TimeSpan.FromMinutes(1);

    public CachedAuctionRepository(IAuctionRepository inner, ICacheService cache, IMapper mapper, IAppLogger<CachedAuctionRepository> logger)
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

    private async Task InvalidateAfterWrite(Guid id, CancellationToken cancellationToken)
    {
        await _cache.RemoveAsync(CacheKeys.Auction(id), cancellationToken);
        await _cache.RemoveAsync(CacheKeys.AuctionList(), cancellationToken);
    }
}