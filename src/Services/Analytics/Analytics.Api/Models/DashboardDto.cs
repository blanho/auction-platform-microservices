namespace Analytics.Api.Models;

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
    public string ApiStatus { get; set; } = HealthStatus.Healthy;
    public string DatabaseStatus { get; set; } = HealthStatus.Unknown;
    public string CacheStatus { get; set; } = HealthStatus.Unknown;
    public string QueueStatus { get; set; } = HealthStatus.Unknown;
    public int QueueJobCount { get; set; }
}

public class AdminDashboardStatsDto
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

public class AdminRecentActivityDto
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; }
    public string Status { get; set; } = "info";
    public string? RelatedEntityId { get; set; }
}

public class PlatformHealthDto
{
    public string ApiStatus { get; set; } = HealthStatus.Healthy;
    public string DatabaseStatus { get; set; } = HealthStatus.Connected;
    public string CacheStatus { get; set; } = HealthStatus.Healthy;
    public int QueueJobCount { get; set; }
    public string QueueStatus { get; set; } = HealthStatus.Healthy;
}

public static class HealthStatus
{
    public const string Healthy = "healthy";
    public const string Unknown = "unknown";
    public const string Connected = "connected";
    public const string Disconnected = "disconnected";
    public const string ServiceUnavailable = "service_unavailable";
    public const string AuctionServiceUnavailable = "auction_service_unavailable";
}
