using Common.Audit.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Common.Core.Authorization;
using AnalyticsService.DTOs;
using AnalyticsService.Interfaces;

namespace AnalyticsService.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/[controller]")]
[HasPermission(Permissions.Analytics.ViewPlatform)]
public class DashboardController : ControllerBase
{
    private readonly IDashboardStatsService _dashboardStatsService;
    private readonly IAuditLogRepository _auditLogRepository;

    public DashboardController(
        IDashboardStatsService dashboardStatsService,
        IAuditLogRepository auditLogRepository)
    {
        _dashboardStatsService = dashboardStatsService;
        _auditLogRepository = auditLogRepository;
    }

    [HttpGet("stats")]
    public async Task<ActionResult<AdminDashboardStatsDto>> GetStats(CancellationToken cancellationToken)
    {
        var stats = await _dashboardStatsService.GetStatsAsync(cancellationToken);
        
        var dto = new AdminDashboardStatsDto
        {
            TotalRevenue = stats.TotalRevenue,
            RevenueChange = stats.RevenueChange,
            ActiveUsers = stats.ActiveUsers,
            ActiveUsersChange = stats.ActiveUsersChange,
            LiveAuctions = stats.LiveAuctions,
            LiveAuctionsChange = stats.LiveAuctionsChange,
            PendingReports = stats.PendingReports,
            PendingReportsChange = stats.PendingReportsChange,
            TotalOrders = stats.TotalOrders,
            CompletedOrders = stats.CompletedOrders
        };

        return Ok(dto);
    }

    [HttpGet("activity")]
    public async Task<ActionResult<List<AdminRecentActivityDto>>> GetRecentActivity(
        [FromQuery] int limit = AnalyticsDefaults.DefaultLimit,
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
    public async Task<ActionResult<PlatformHealthDto>> GetPlatformHealth(CancellationToken cancellationToken)
    {
        var healthStatus = await _dashboardStatsService.GetHealthStatusAsync(cancellationToken);
        
        var dto = new PlatformHealthDto
        {
            ApiStatus = healthStatus.ApiStatus,
            DatabaseStatus = healthStatus.DatabaseStatus,
            CacheStatus = healthStatus.CacheStatus,
            QueueStatus = healthStatus.QueueStatus,
            QueueJobCount = healthStatus.QueueJobCount
        };

        return Ok(dto);
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
