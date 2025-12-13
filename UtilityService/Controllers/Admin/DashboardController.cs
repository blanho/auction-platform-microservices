using Common.Audit.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UtilityService.Interfaces;

namespace UtilityService.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/[controller]")]
[Authorize(Roles = "Admin")]
public class DashboardController : ControllerBase
{
    private readonly IReportRepository _reportRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IAuditLogRepository _auditLogRepository;

    public DashboardController(
        IReportRepository reportRepository,
        IWalletRepository walletRepository,
        IAuditLogRepository auditLogRepository)
    {
        _reportRepository = reportRepository;
        _walletRepository = walletRepository;
        _auditLogRepository = auditLogRepository;
    }

    [HttpGet("stats")]
    public async Task<ActionResult<AdminDashboardStatsDto>> GetStats(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var startOfCurrentMonth = new DateTime(now.Year, now.Month, 1);
        var startOfPreviousMonth = startOfCurrentMonth.AddMonths(-1);
        var endOfPreviousMonth = startOfCurrentMonth.AddDays(-1);

        var currentMonthRevenue = await _walletRepository.GetTotalRevenueAsync(startOfCurrentMonth, now, cancellationToken);
        var previousMonthRevenue = await _walletRepository.GetTotalRevenueAsync(startOfPreviousMonth, endOfPreviousMonth, cancellationToken);
        var revenueChange = previousMonthRevenue > 0 
            ? ((currentMonthRevenue - previousMonthRevenue) / previousMonthRevenue) * 100 
            : 0;

        var activeUsers = await _walletRepository.GetActiveUsersCountAsync(30, cancellationToken);
        var previousPeriodActiveUsers = await _walletRepository.GetActiveUsersCountAsync(60, cancellationToken);
        var previousActiveUsers = previousPeriodActiveUsers - activeUsers;
        var activeUsersChange = previousActiveUsers > 0 
            ? ((decimal)(activeUsers - previousActiveUsers) / previousActiveUsers) * 100 
            : 0;

        var pendingReportsCount = await _reportRepository.GetPendingCountAsync(cancellationToken);
        
        var stats = new AdminDashboardStatsDto
        {
            TotalRevenue = currentMonthRevenue,
            RevenueChange = Math.Round(revenueChange, 1),
            ActiveUsers = activeUsers,
            ActiveUsersChange = Math.Round(activeUsersChange, 1),
            LiveAuctions = 0,
            LiveAuctionsChange = 0,
            PendingReports = pendingReportsCount,
            PendingReportsChange = 0,
            TotalOrders = 0,
            CompletedOrders = 0
        };

        return Ok(stats);
    }

    [HttpGet("activity")]
    public async Task<ActionResult<List<AdminRecentActivityDto>>> GetRecentActivity(
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        var auditLogs = await _auditLogRepository.GetRecentAsync(limit, cancellationToken);
        
        var activities = auditLogs.Select(log => new AdminRecentActivityDto
        {
            Id = log.Id.ToString(),
            Type = MapAuditActionToType(log.Action),
            Message = $"{log.Action} on {log.EntityType}",
            Timestamp = log.Timestamp,
            Status = "info",
            RelatedEntityId = log.EntityId.ToString()
        }).ToList();

        return Ok(activities);
    }

    [HttpGet("health")]
    [AllowAnonymous]
    public ActionResult<PlatformHealthDto> GetPlatformHealth()
    {
        var health = new PlatformHealthDto
        {
            ApiStatus = "healthy",
            DatabaseStatus = "connected",
            CacheStatus = "active",
            QueueJobCount = 0,
            QueueStatus = "healthy"
        };

        return Ok(health);
    }

    private static string MapAuditActionToType(AuditAction action)
    {
        return action switch
        {
            AuditAction.Created => "auction",
            AuditAction.Updated => "user",
            AuditAction.Deleted => "report",
            _ => "info"
        };
    }
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
    public string ApiStatus { get; set; } = "healthy";
    public string DatabaseStatus { get; set; } = "connected";
    public string CacheStatus { get; set; } = "active";
    public int QueueJobCount { get; set; }
    public string QueueStatus { get; set; } = "healthy";
}
