using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using BuildingBlocks.Application.Abstractions;
using Notification.Application.DTOs;
using Notification.Domain.Enums;
using BuildingBlocks.Application.Constants;
using BuildingBlocks.Web.Authorization;
using BuildingBlocks.Web.Extensions;
using System.Security.Claims;
using Notification.Application.Features.Notifications.GetUserNotifications;
using Notification.Application.Features.Notifications.GetNotificationSummary;
using Notification.Application.Features.Notifications.MarkAsRead;
using Notification.Application.Features.Notifications.MarkAllAsRead;
using Notification.Application.Features.Notifications.DeleteNotification;
using Notification.Application.Features.Notifications.ArchiveNotification;
using Notification.Application.Features.Notifications.CreateNotification;
using Notification.Application.Features.Notifications.BroadcastNotification;
using Notification.Application.Features.Notifications.GetNotificationStats;
using Notification.Application.Features.Preferences.GetPreferences;
using Notification.Application.Features.Preferences.UpdatePreferences;

namespace Notification.Api.Endpoints;

public class NotificationEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/notifications")
            .WithTags("Notifications")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Notifications.View));

        group.MapGet("/", GetMyNotifications)
            .WithName("GetMyNotifications")
            .WithSummary("Get notifications for the current user");

        group.MapGet("/summary", GetSummary)
            .WithName("GetNotificationSummary")
            .WithSummary("Get notification summary for the current user");

        group.MapGet("/unread", GetUnreadNotifications)
            .WithName("GetUnreadNotifications")
            .WithSummary("Get unread notifications for the current user");

        group.MapPut("/{id:guid}/read", MarkAsRead)
            .WithName("MarkNotificationAsRead")
            .WithSummary("Mark a notification as read")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Notifications.View));

        group.MapPut("/read-all", MarkAllAsRead)
            .WithName("MarkAllNotificationsAsRead")
            .WithSummary("Mark all notifications as read")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Notifications.View));

        group.MapDelete("/{id:guid}", Delete)
            .WithName("DeleteNotification")
            .WithSummary("Delete a notification")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Notifications.View));

        group.MapPost("/{id:guid}/archive", Archive)
            .WithName("ArchiveNotification")
            .WithSummary("Archive a notification")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Notifications.View));

        group.MapGet("/preferences", GetPreferences)
            .WithName("GetNotificationPreferences")
            .WithSummary("Get notification preferences for the current user");

        group.MapPut("/preferences", UpdatePreferences)
            .WithName("UpdateNotificationPreferences")
            .WithSummary("Update notification preferences for the current user");

        group.MapPost("/", Create)
            .WithName("CreateNotification")
            .WithSummary("Create a notification")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Notifications.Send));

        var adminGroup = app.MapGroup("/api/v1/notifications/admin")
            .WithTags("Notifications Admin")
            .WithMetadata(new RequireAdminAttribute());

        adminGroup.MapGet("/all", GetAllNotifications)
            .WithName("GetAllNotifications")
            .WithSummary("Get all notifications (admin)");

        adminGroup.MapPost("/broadcast", BroadcastNotification)
            .WithName("BroadcastNotification")
            .WithSummary("Broadcast notification to all users");

        adminGroup.MapGet("/stats", GetNotificationStats)
            .WithName("GetNotificationStats")
            .WithSummary("Get notification statistics");
    }

    private static async Task<IResult> GetMyNotifications(
        ClaimsPrincipal user,
        ISender sender,
        [AsParameters] PaginationRequest pagination,
        CancellationToken cancellationToken)
    {
        var userId = user.GetRequiredUserIdString();
        var query = new GetUserNotificationsQuery(
            userId,
            pagination.Page ?? PaginationDefaults.DefaultPage,
            pagination.PageSize ?? PaginationDefaults.DefaultPageSize);
        var result = await sender.Send(query, cancellationToken);

        return result.ToOkResult();
    }

    private static async Task<IResult> GetSummary(
        ClaimsPrincipal user,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var userId = user.GetRequiredUserIdString();
        var query = new GetNotificationSummaryQuery(userId);
        var result = await sender.Send(query, cancellationToken);

        return result.ToOkResult();
    }

    private static async Task<IResult> GetUnreadNotifications(
        ClaimsPrincipal user,
        ISender sender,
        [AsParameters] PaginationRequest pagination,
        CancellationToken cancellationToken)
    {
        var userId = user.GetRequiredUserIdString();
        var query = new GetUserNotificationsQuery(
            userId,
            pagination.Page ?? PaginationDefaults.DefaultPage,
            pagination.PageSize ?? PaginationDefaults.DefaultPageSize);
        var result = await sender.Send(query, cancellationToken);

        return result.ToApiResult(paginatedResult =>
        {
            var unreadOnly = new PaginatedResult<NotificationDto>(
                paginatedResult.Items.Where(n => n.Status == nameof(NotificationStatus.Unread)).ToList(),
                paginatedResult.TotalCount,
                paginatedResult.Page,
                paginatedResult.PageSize);

            return Results.Ok(unreadOnly);
        });
    }

    private static async Task<IResult> MarkAsRead(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new MarkAsReadCommand(id);
        var result = await sender.Send(command, cancellationToken);

        return result.ToNoContentResult();
    }

    private static async Task<IResult> MarkAllAsRead(
        ClaimsPrincipal user,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var userId = user.GetRequiredUserIdString();
        var command = new MarkAllAsReadCommand(userId);
        var result = await sender.Send(command, cancellationToken);

        return result.ToNoContentResult();
    }

    private static async Task<IResult> Delete(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new DeleteNotificationCommand(id);
        var result = await sender.Send(command, cancellationToken);

        return result.ToNoContentResult();
    }

    private static async Task<IResult> Archive(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new ArchiveNotificationCommand(id);
        var result = await sender.Send(command, cancellationToken);

        return result.ToNoContentResult();
    }

    private static async Task<IResult> GetPreferences(
        ClaimsPrincipal user,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var userId = user.GetRequiredUserIdString();
        var query = new GetPreferencesQuery(userId);
        var result = await sender.Send(query, cancellationToken);

        return result.ToOkResult();
    }

    private static async Task<IResult> UpdatePreferences(
        [FromBody] UpdatePreferencesRequest request,
        ClaimsPrincipal user,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var userId = user.GetRequiredUserIdString();
        var command = new UpdatePreferencesCommand(
            userId,
            request.EmailEnabled,
            request.PushEnabled,
            request.BidUpdates,
            request.AuctionUpdates,
            request.PromotionalEmails,
            request.SystemAlerts);
        var result = await sender.Send(command, cancellationToken);

        return result.ToOkResult();
    }

    private static async Task<IResult> Create(
        [FromBody] CreateNotificationRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new CreateNotificationCommand(
            request.UserId,
            request.Type.ToString(),
            request.Title,
            request.Message,
            request.Data,
            request.AuctionId,
            request.BidId);
        var result = await sender.Send(command, cancellationToken);

        return result.ToApiResult(value =>
            Results.Created($"/api/v1/notifications/{value.Id}", value));
    }

    private static async Task<IResult> GetAllNotifications(
        [AsParameters] GetAllNotificationsRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetUserNotificationsQuery(
            request.UserId ?? string.Empty,
            request.Page ?? PaginationDefaults.DefaultPage,
            request.PageSize ?? PaginationDefaults.LargePageSize);
        var result = await sender.Send(query, cancellationToken);

        return result.ToOkResult();
    }

    private static async Task<IResult> BroadcastNotification(
        [FromBody] BroadcastNotificationRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new BroadcastNotificationCommand(
            request.Type.ToString(),
            request.Title,
            request.Message,
            request.TargetRole);
        var result = await sender.Send(command, cancellationToken);

        return result.ToOkResult();
    }

    private static async Task<IResult> GetNotificationStats(
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetNotificationStatsQuery();
        var result = await sender.Send(query, cancellationToken);

        return result.ToOkResult();
    }
}
