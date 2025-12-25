using AnalyticsService.DTOs;

namespace AnalyticsService.Interfaces;

public interface IDashboardStatsService
{
    Task<DashboardStats> GetStatsAsync(CancellationToken cancellationToken = default);
    Task<PlatformHealthStatus> GetHealthStatusAsync(CancellationToken cancellationToken = default);
}
