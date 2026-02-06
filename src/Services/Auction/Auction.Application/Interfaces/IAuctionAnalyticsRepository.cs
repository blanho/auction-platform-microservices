using Auctions.Application.DTOs;
using Auctions.Domain.Entities;
using BuildingBlocks.Domain.Enums;

namespace Auctions.Application.Interfaces;

public interface IAuctionAnalyticsRepository
{
    Task<int> CountLiveAuctionsAsync(CancellationToken cancellationToken = default);
    Task<int> CountEndingSoonAsync(CancellationToken cancellationToken = default);
    Task<int> GetCountByStatusAsync(Status status, CancellationToken cancellationToken = default);
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);
    Task<int> GetCountEndingBetweenAsync(DateTimeOffset start, DateTimeOffset end, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalRevenueAsync(CancellationToken cancellationToken = default);
    Task<List<Auction>> GetTrendingItemsAsync(int limit, CancellationToken cancellationToken = default);
    Task<List<Auction>> GetTopByRevenueAsync(int limit, CancellationToken cancellationToken = default);
    Task<List<CategoryStatDto>> GetCategoryStatsAsync(CancellationToken cancellationToken = default);
}
