using Carter;
using Microsoft.AspNetCore.Http.HttpResults;
using Analytics.Api.Models;
using Analytics.Api.Interfaces;
using BuildingBlocks.Web.Authorization;
using Common.Contracts.Events;

namespace Analytics.Api.Endpoints;

public class DashboardEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/dashboard")
            .WithTags("Dashboard");

        group.MapGet("/stats", GetDashboardStats)
            .WithName("GetDashboardStats")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Analytics.ViewPlatform))
            .Produces<AdminDashboardStatsDto>();

        group.MapGet("/activity", GetRecentActivity)
            .WithName("GetRecentActivity")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.AuditLogs.View))
            .Produces<List<AdminRecentActivityDto>>();

        group.MapGet("/health", GetPlatformHealth)
            .WithName("GetPlatformHealth")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Analytics.ViewPlatform))
            .Produces<PlatformHealthDto>();
    }

    private static async Task<Ok<AdminDashboardStatsDto>> GetDashboardStats(
        IDashboardStatsService dashboardStatsService,
        CancellationToken cancellationToken)
    {
        var stats = await dashboardStatsService.GetStatsAsync(cancellationToken);

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

        return TypedResults.Ok(dto);
    }

    private static async Task<Ok<List<AdminRecentActivityDto>>> GetRecentActivity(
        int? limit,
        IAuditLogRepository auditLogRepository,
        CancellationToken cancellationToken)
    {
        var auditLogs = await auditLogRepository.GetRecentAsync(limit ?? AnalyticsDefaults.DefaultLimit, cancellationToken);

        var activities = auditLogs.Select(log => new AdminRecentActivityDto
        {
            Id = log.Id.ToString(),
            Type = MapAuditActionToType(log.Action),
            Message = $"{log.Action} on {log.EntityType}",
            Timestamp = log.Timestamp,
            Status = "info",
            RelatedEntityId = log.EntityId.ToString()
        }).ToList();

        return TypedResults.Ok(activities);
    }

    private static async Task<Ok<PlatformHealthDto>> GetPlatformHealth(
        IDashboardStatsService dashboardStatsService,
        CancellationToken cancellationToken)
    {
        var healthStatus = await dashboardStatsService.GetHealthStatusAsync(cancellationToken);

        var dto = new PlatformHealthDto
        {
            ApiStatus = healthStatus.ApiStatus,
            DatabaseStatus = healthStatus.DatabaseStatus,
            CacheStatus = healthStatus.CacheStatus,
            QueueStatus = healthStatus.QueueStatus,
            QueueJobCount = healthStatus.QueueJobCount
        };

        return TypedResults.Ok(dto);
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
