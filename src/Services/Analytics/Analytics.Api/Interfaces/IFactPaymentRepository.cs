using Analytics.Api.Models;

namespace Analytics.Api.Interfaces;

public interface IFactPaymentRepository
{
    Task<RevenueMetrics> GetRevenueMetricsAsync(
        DateTimeOffset? startDate,
        DateTimeOffset? endDate,
        CancellationToken cancellationToken = default);

    Task<List<TrendDataPoint>> GetRevenueTrendAsync(
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        CancellationToken cancellationToken = default);

    Task<List<TopSellerDto>> GetTopSellersAsync(
        int limit,
        DateTimeOffset? startDate,
        CancellationToken cancellationToken = default);

    Task<List<TopBuyerDto>> GetTopBuyersAsync(
        int limit,
        DateTimeOffset? startDate,
        CancellationToken cancellationToken = default);
}
