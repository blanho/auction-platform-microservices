using Auctions.Application.DTOs;
using Auctions.Domain.Entities;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Domain.Enums;

namespace Auctions.Application.Interfaces;

[Obsolete("Use focused interfaces instead: IAuctionQueryRepository, IAuctionSchedulerRepository, IAuctionAnalyticsRepository, IAuctionUserRepository, IAuctionExportRepository")]
public interface IAuctionReadRepository
{
    Task<Auction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PaginatedResult<Auction>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<PaginatedResult<Auction>> GetPagedAsync(AuctionFilterDto filter, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Auction>> GetFinishedAuctionsAsync(CancellationToken cancellationToken = default);
    Task<List<Auction>> GetAuctionsToAutoDeactivateAsync(CancellationToken cancellationToken = default);
    Task<List<Auction>> GetScheduledAuctionsToActivateAsync(CancellationToken cancellationToken = default);
    Task<List<Auction>> GetAuctionsEndingBetweenAsync(DateTime startTime, DateTime endTime, CancellationToken cancellationToken = default);
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
    Task<List<Auction>> GetBySellerUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<List<Auction>> GetWonByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<SellerStatsDto> GetSellerStatsAsync(
        string username,
        DateTimeOffset periodStart,
        DateTimeOffset? previousPeriodStart = null,
        CancellationToken cancellationToken = default);
    Task<List<Auction>> GetActiveAuctionsBySellerIdAsync(Guid sellerId, CancellationToken cancellationToken = default);
    Task<List<Auction>> GetAllBySellerIdAsync(Guid sellerId, CancellationToken cancellationToken = default);
    Task<List<Auction>> GetAuctionsWithWinnerIdAsync(Guid winnerId, CancellationToken cancellationToken = default);
    Task<int> GetWatchlistCountByUsernameAsync(string username, CancellationToken cancellationToken = default);
}
