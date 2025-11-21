using AuctionService.Application.Interfaces;
using AuctionService.Domain.Entities;
using AutoMapper;
using Common.Caching.Abstractions;
using Common.Caching.Keys;

namespace AuctionService.Infrastructure.Repositories;

public class CachedAuctionRepository : IAuctionRepository
{
    private readonly IAuctionRepository _inner;
    private readonly ICacheService _cache;
    private readonly IMapper _mapper;
    private static readonly TimeSpan SingleAuctionTtl = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan AuctionListTtl = TimeSpan.FromMinutes(1);

    public CachedAuctionRepository(IAuctionRepository inner, ICacheService cache, IMapper mapper)
    {
        _inner = inner;
        _cache = cache;
        _mapper = mapper;
    }

    public async Task<List<Auction>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var key = CacheKeys.AuctionList();
        var cachedDtos = await _cache.GetAsync<List<Application.DTOs.AuctionDto>>(key, cancellationToken);
        if (cachedDtos != null)
        {
            return _mapper.Map<List<Auction>>(cachedDtos);
        }

        var auctions = await _inner.GetAllAsync(cancellationToken);
        var dtos = _mapper.Map<List<Application.DTOs.AuctionDto>>(auctions);
        await _cache.SetAsync(key, dtos, AuctionListTtl, cancellationToken);
        return auctions;
    }

    public async Task<Auction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var key = CacheKeys.Auction(id);
        var cachedDto = await _cache.GetAsync<Application.DTOs.AuctionDto>(key, cancellationToken);
        if (cachedDto != null)
        {
            return _mapper.Map<Auction>(cachedDto);
        }

        var auction = await _inner.GetByIdAsync(id, cancellationToken);
        if (auction != null)
        {
            var dto = _mapper.Map<Application.DTOs.AuctionDto>(auction);
            await _cache.SetAsync(key, dto, SingleAuctionTtl, cancellationToken);
        }
        return auction;
    }

    public async Task<Auction> CreateAsync(Auction auction, CancellationToken cancellationToken = default)
    {
        var result = await _inner.CreateAsync(auction, cancellationToken);
        await InvalidateAfterWrite(result.Id, cancellationToken);
        return result;
    }

    public async Task<IEnumerable<Auction>> AddRangeAsync(IEnumerable<Auction> auctions, CancellationToken cancellationToken = default)
    {
        var result = await _inner.AddRangeAsync(auctions, cancellationToken);
        foreach (var a in result)
        {
            await InvalidateAfterWrite(a.Id, cancellationToken);
        }
        return result;
    }

    public async Task UpdateAsync(Auction auction, CancellationToken cancellationToken = default)
    {
        await _inner.UpdateAsync(auction, cancellationToken);
        await InvalidateAfterWrite(auction.Id, cancellationToken);
    }

    public async Task UpdateRangeAsync(IEnumerable<Auction> auctions, CancellationToken cancellationToken = default)
    {
        await _inner.UpdateRangeAsync(auctions, cancellationToken);
        foreach (var a in auctions)
        {
            await InvalidateAfterWrite(a.Id, cancellationToken);
        }
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _inner.DeleteAsync(id, cancellationToken);
        await InvalidateAfterWrite(id, cancellationToken);
    }

    public async Task DeleteRangeAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        await _inner.DeleteRangeAsync(ids, cancellationToken);
        foreach (var id in ids)
        {
            await InvalidateAfterWrite(id, cancellationToken);
        }
    }

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        => _inner.ExistsAsync(id, cancellationToken);

    private async Task InvalidateAfterWrite(Guid id, CancellationToken cancellationToken)
    {
        await _cache.RemoveAsync(CacheKeys.Auction(id), cancellationToken);
        await _cache.RemoveAsync(CacheKeys.AuctionList(), cancellationToken);
    }
}