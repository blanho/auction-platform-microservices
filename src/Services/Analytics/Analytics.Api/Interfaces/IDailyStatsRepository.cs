using Analytics.Api.Models;

namespace Analytics.Api.Interfaces;

/// <summary>
/// Repository interface for querying pre-aggregated daily statistics views.
/// </summary>
public interface IDailyStatsRepository
{
    /// <summary>
    /// Gets daily auction statistics within the specified date range.
    /// </summary>
    Task<List<DailyAuctionStatsDto>> GetDailyAuctionStatsAsync(
        DateOnly? startDate,
        DateOnly? endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets daily bid statistics within the specified date range.
    /// </summary>
    Task<List<DailyBidStatsDto>> GetDailyBidStatsAsync(
        DateOnly? startDate,
        DateOnly? endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets daily revenue statistics within the specified date range.
    /// </summary>
    Task<List<DailyRevenueStatsDto>> GetDailyRevenueStatsAsync(
        DateOnly? startDate,
        DateOnly? endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets combined aggregated statistics from all three views for the specified date range.
    /// </summary>
    Task<AggregatedDailyStatsDto> GetAggregatedStatsAsync(
        DateOnly? startDate,
        DateOnly? endDate,
        CancellationToken cancellationToken = default);
}
