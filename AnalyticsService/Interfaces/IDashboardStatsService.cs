namespace AnalyticsService.Interfaces;

public interface IDashboardStatsService
{
    Task<DashboardStats> GetStatsAsync(CancellationToken cancellationToken = default);
    Task<PlatformHealthStatus> GetHealthStatusAsync(CancellationToken cancellationToken = default);
}

public class DashboardStats
{
    public decimal TotalRevenue { get; set; }
    public decimal RevenueChange { get; set; }
    public int ActiveUsers { get; set; }
    public decimal ActiveUsersChange { get; set; }
    public int LiveAuctions { get; set; }
    public decimal LiveAuctionsChange { get; set; }
    public int PendingReports { get; set; }
    public decimal PendingReportsChange { get; set; }
    public int TotalOrders { get; set; }
    public int CompletedOrders { get; set; }
}

public class PlatformHealthStatus
{
    public string ApiStatus { get; set; } = "healthy";
    public string DatabaseStatus { get; set; } = "unknown";
    public string CacheStatus { get; set; } = "unknown";
    public string QueueStatus { get; set; } = "unknown";
    public int QueueJobCount { get; set; }
}
