using Analytics.Application.DTOs;

namespace Analytics.Application.Interfaces;

public interface IDailyStatsRepository
{
    Task<List<DailyAuctionStatsDto>> GetDailyAuctionStatsAsync(
        DateOnly? startDate,
        DateOnly? endDate,
        CancellationToken cancellationToken = default);

    Task<List<DailyBidStatsDto>> GetDailyBidStatsAsync(
        DateOnly? startDate,
        DateOnly? endDate,
        CancellationToken cancellationToken = default);

    Task<List<DailyRevenueStatsDto>> GetDailyRevenueStatsAsync(
        DateOnly? startDate,
        DateOnly? endDate,
        CancellationToken cancellationToken = default);

    Task<AggregatedDailyStatsDto> GetAggregatedStatsAsync(
        DateOnly? startDate,
        DateOnly? endDate,
        CancellationToken cancellationToken = default);
}
