using Carter;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Analytics.Application.DTOs;
using Analytics.Application.Features.AuditLogs.GetPagedAuditLogs;
using Analytics.Application.Features.PlatformAnalytics.GetPlatformAnalytics;
using BuildingBlocks.Web.Authorization;

namespace Analytics.Api.Endpoints.Platform;

public class DashboardEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/dashboard")
            .WithTags("Dashboard");

        group.MapGet("/stats", GetDashboardStats)
            .WithName("GetPlatformAnalytics")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Analytics.ViewPlatform))
            .Produces<PlatformAnalyticsDto>();

        group.MapGet("/activity", GetRecentActivity)
            .WithName("GetRecentActivity")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.AuditLogs.View))
            .Produces<List<RecentActivityDto>>();
    }

    private static async Task<Ok<PlatformAnalyticsDto>> GetDashboardStats(
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new AnalyticsQueryParams();
        var stats = await sender.Send(new GetPlatformAnalyticsQuery(query), cancellationToken);
        return TypedResults.Ok(stats);
    }

    private static async Task<Ok<List<RecentActivityDto>>> GetRecentActivity(
        int? limit,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new AuditLogQueryParams
        {
            PageSize = limit ?? AnalyticsDefaults.DefaultPageSize,
            SortBy = "Timestamp",
            SortDescending = true
        };
        var auditLogs = await sender.Send(new GetPagedAuditLogsQuery(query), cancellationToken);

        var activities = auditLogs.Items.Select(log => new RecentActivityDto
        {
            Type = log.EntityType.ToLowerInvariant(),
            Description = $"{log.Action} on {log.EntityType}",
            Timestamp = log.Timestamp,
            RelatedEntityId = log.EntityId,
            RelatedEntityType = log.EntityType
        }).ToList();

        return TypedResults.Ok(activities);
    }
}
