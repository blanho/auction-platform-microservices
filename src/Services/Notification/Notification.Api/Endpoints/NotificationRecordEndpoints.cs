using Carter;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Constants;
using BuildingBlocks.Web.Authorization;
using Notification.Application.DTOs;
using Notification.Application.Features.NotificationRecords.GetNotificationRecords;
using Notification.Application.Features.NotificationRecords.GetNotificationRecordById;
using Notification.Application.Features.NotificationRecords.GetNotificationRecordsByUser;
using Notification.Application.Features.NotificationRecords.GetNotificationRecordStats;

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

    private static async Task<Ok<PaginatedResult<NotificationRecordDto>>> GetRecords(
        [AsParameters] NotificationRecordFilterRequest request,
        ISender sender,
        CancellationToken ct)
    {
        var query = new GetNotificationRecordsQuery(
            request.UserId,
            request.Channel,
            request.Status,
            request.TemplateKey,
            request.FromDate,
            request.ToDate,
            request.Page ?? 1,
            request.PageSize ?? 20);

        var result = await sender.Send(query, ct);
        return TypedResults.Ok(result.Value);
    }

    private static async Task<Results<Ok<NotificationRecordDto>, NotFound>> GetRecordById(
        Guid id,
        ISender sender,
        CancellationToken ct)
    {
        var query = new GetNotificationRecordByIdQuery(id);
        var result = await sender.Send(query, ct);
        
        return result.Value is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(result.Value);
    }

    private static async Task<Ok<PaginatedResult<NotificationRecordDto>>> GetRecordsByUser(
        Guid userId,
        string? channel,
        int page,
        int pageSize,
        ISender sender,
        CancellationToken ct)
    {
        var query = new GetNotificationRecordsByUserQuery(userId, channel, Page: page, PageSize: pageSize);
        var result = await sender.Send(query, ct);
        
        return TypedResults.Ok(result.Value);
    }

    private static async Task<Ok<NotificationRecordStatsDto>> GetRecordStats(
        ISender sender,
        CancellationToken ct)
    {
        var query = new GetNotificationRecordStatsQuery();
        var result = await sender.Send(query, ct);
        
        return TypedResults.Ok(result.Value);
    }
}
