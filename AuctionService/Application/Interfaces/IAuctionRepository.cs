using AuctionService.Application.DTOs;
using AuctionService.Domain.Entities;
using Common.Core.Constants;
using Common.Core.Helpers;
using Common.Domain.Enums;
using Common.Repository.Interfaces;

namespace AuctionService.Application.Interfaces;

public interface IAuctionRepository : IRepository<Auction>
{
    Task<(List<Auction> Items, int TotalCount)> GetPagedAsync(
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
        CancellationToken cancellationToken = default);
    
    Task<List<Auction>> GetFinishedAuctionsAsync(CancellationToken cancellationToken = default);
    
    Task<List<Auction>> GetAuctionsToAutoDeactivateAsync(CancellationToken cancellationToken = default);
    
    Task<List<Auction>> GetScheduledAuctionsToActivateAsync(CancellationToken cancellationToken = default);
    
    Task<List<Auction>> GetAuctionsEndingBetweenAsync(
        DateTime startTime, 
        DateTime endTime, 
        CancellationToken cancellationToken = default);
    
    Task<List<Auction>> GetAuctionsForExportAsync(
        Status? status = null,
        string? seller = null,
        DateTimeOffset? startDate = null,
        DateTimeOffset? endDate = null,
        CancellationToken cancellationToken = default);
    
    Task<int> CountLiveAuctionsAsync(CancellationToken cancellationToken = default);
    
    Task<int> CountEndingSoonAsync(CancellationToken cancellationToken = default);
    
    Task<int> GetCountByStatusAsync(Status status, CancellationToken cancellationToken = default);
    
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);
    
    Task<decimal> GetTotalRevenueAsync(CancellationToken cancellationToken = default);
    
    Task<List<Auction>> GetTrendingItemsAsync(int limit, CancellationToken cancellationToken = default);
    
    Task<List<Auction>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
    
    Task<int> GetCountEndingBetweenAsync(DateTimeOffset start, DateTimeOffset end, CancellationToken cancellationToken = default);
    
    Task<List<Auction>> GetTopByRevenueAsync(int limit, CancellationToken cancellationToken = default);
    
    Task<List<CategoryStatDto>> GetCategoryStatsAsync(CancellationToken cancellationToken = default);
}
