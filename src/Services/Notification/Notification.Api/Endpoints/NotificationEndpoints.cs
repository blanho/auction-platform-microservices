using Carter;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using BuildingBlocks.Application.Abstractions;
using Notification.Application.DTOs;
using Notification.Application.Interfaces;
using Notification.Domain.Enums;
using BuildingBlocks.Application.Constants;
using BuildingBlocks.Web.Authorization;
using System.Security.Claims;

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

    private static string GetUserId(ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? user.FindFirst("sub")?.Value
            ?? user.FindFirst("username")?.Value
            ?? throw new UnauthorizedAccessException("User ID not found in token");
    }

    private static async Task<Ok<List<NotificationDto>>> GetMyNotifications(
        ClaimsPrincipal user,
        INotificationService notificationService,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        var notifications = await notificationService.GetUserNotificationsAsync(userId, cancellationToken);
        return TypedResults.Ok(notifications);
    }

    private static async Task<Ok<NotificationSummaryDto>> GetSummary(
        ClaimsPrincipal user,
        INotificationService notificationService,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        var summary = await notificationService.GetNotificationSummaryAsync(userId, cancellationToken);
        return TypedResults.Ok(summary);
    }

    private static async Task<Ok<List<NotificationDto>>> GetUnreadNotifications(
        ClaimsPrincipal user,
        INotificationService notificationService,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        var notifications = await notificationService.GetUserNotificationsAsync(userId, cancellationToken);
        var unread = notifications.Where(n => n.Status == nameof(NotificationStatus.Unread)).ToList();
        return TypedResults.Ok(unread);
    }

    private static async Task<Results<NoContent, NotFound>> MarkAsRead(
        Guid id,
        ClaimsPrincipal user,
        INotificationService notificationService,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        var notifications = await notificationService.GetUserNotificationsAsync(userId, cancellationToken);
        var notification = notifications.FirstOrDefault(n => n.Id == id);

        if (notification == null)
        {
            return TypedResults.NotFound();
        }

        await notificationService.MarkAsReadAsync(id, cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<NoContent> MarkAllAsRead(
        ClaimsPrincipal user,
        INotificationService notificationService,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        await notificationService.MarkAllAsReadAsync(userId, cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<Results<NoContent, NotFound>> Delete(
        Guid id,
        ClaimsPrincipal user,
        INotificationService notificationService,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        var notifications = await notificationService.GetUserNotificationsAsync(userId, cancellationToken);
        var notification = notifications.FirstOrDefault(n => n.Id == id);

        if (notification == null)
        {
            return TypedResults.NotFound();
        }

        await notificationService.DeleteNotificationAsync(id, cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<Created<NotificationDto>> Create(
        CreateNotificationDto dto,
        INotificationService notificationService,
        CancellationToken cancellationToken)
    {
        var notification = await notificationService.CreateNotificationAsync(dto, cancellationToken);
        return TypedResults.Created($"/api/v1/notifications/{notification.Id}", notification);
    }

    private static async Task<Ok<PaginatedResult<NotificationDto>>> GetAllNotifications(
        [AsParameters] GetAllNotificationsRequest request,
        INotificationService notificationService,
        CancellationToken cancellationToken)
    {
        var notifications = await notificationService.GetAllNotificationsAsync(
            request.Page ?? PaginationDefaults.DefaultPage,
            request.PageSize ?? PaginationDefaults.LargePageSize,
            request.UserId,
            request.Type,
            request.Status,
            cancellationToken);
        return TypedResults.Ok(notifications);
    }

    private static async Task<Ok<object>> BroadcastNotification(
        BroadcastNotificationDto dto,
        INotificationService notificationService,
        CancellationToken cancellationToken)
    {
        await notificationService.BroadcastNotificationAsync(dto, cancellationToken);
        return TypedResults.Ok(new { message = "Notification broadcast successfully" } as object);
    }

    private static async Task<Ok<NotificationStatsDto>> GetNotificationStats(
        INotificationService notificationService,
        CancellationToken cancellationToken)
    {
        var stats = await notificationService.GetNotificationStatsAsync(cancellationToken);
        return TypedResults.Ok(stats);
    }
}

public record GetAllNotificationsRequest(
    int? Page,
    int? PageSize,
    string? UserId,
    string? Type,
    string? Status);
