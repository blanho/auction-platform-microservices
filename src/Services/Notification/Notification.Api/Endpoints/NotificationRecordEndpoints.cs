using Carter;
using Microsoft.AspNetCore.Http.HttpResults;
using BuildingBlocks.Application.Constants;
using BuildingBlocks.Web.Authorization;
using Notification.Application.DTOs;
using Notification.Application.Interfaces;

namespace Notification.Api.Endpoints;

public class NotificationRecordEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/notifications/records")
            .WithTags("Notification Records")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Notifications.ManageTemplates));

        group.MapGet("/", GetRecords)
            .WithName("GetNotificationRecords")
            .WithSummary("Get notification records with filters (paged)");

        group.MapGet("/{id:guid}", GetRecordById)
            .WithName("GetNotificationRecordById")
            .WithSummary("Get a notification record by ID");

        group.MapGet("/user/{userId:guid}", GetRecordsByUser)
            .WithName("GetNotificationRecordsByUser")
            .WithSummary("Get notification records for a specific user");

        group.MapGet("/stats", GetRecordStats)
            .WithName("GetNotificationRecordStats")
            .WithSummary("Get notification record statistics");
    }

    private static async Task<Ok<BuildingBlocks.Application.Abstractions.PaginatedResult<NotificationRecordDto>>> GetRecords(
        Guid? userId,
        string? channel,
        string? status,
        string? templateKey,
        DateTimeOffset? fromDate,
        DateTimeOffset? toDate,
        int page,
        int pageSize,
        INotificationRecordService recordService,
        CancellationToken ct)
    {
        var filter = new NotificationRecordFilterDto
        {
            UserId = userId,
            Channel = channel,
            Status = status,
            TemplateKey = templateKey,
            FromDate = fromDate,
            ToDate = toDate,
            Page = page > 0 ? page : 1,
            PageSize = pageSize > 0 ? pageSize : 20
        };

        var result = await recordService.GetPagedAsync(filter, ct);
        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<NotificationRecordDto>, NotFound>> GetRecordById(
        Guid id,
        INotificationRecordService recordService,
        CancellationToken ct)
    {
        var record = await recordService.GetByIdAsync(id, ct);
        return record is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(record);
    }

    private static async Task<Ok<List<NotificationRecordDto>>> GetRecordsByUser(
        Guid userId,
        int? limit,
        INotificationRecordService recordService,
        CancellationToken ct)
    {
        var records = await recordService.GetByUserIdAsync(userId, limit ?? 50, ct);
        return TypedResults.Ok(records);
    }

    private static async Task<Ok<NotificationRecordStatsDto>> GetRecordStats(
        INotificationRecordService recordService,
        CancellationToken ct)
    {
        var stats = await recordService.GetStatsAsync(ct);
        return TypedResults.Ok(stats);
    }
}
