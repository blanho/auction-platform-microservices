using Analytics.Api.Models;

namespace Analytics.Api.Interfaces;

public interface IDashboardStatsService
{
    Task<DashboardStats> GetStatsAsync(CancellationToken cancellationToken = default);
    Task<PlatformHealthStatus> GetHealthStatusAsync(CancellationToken cancellationToken = default);
}
