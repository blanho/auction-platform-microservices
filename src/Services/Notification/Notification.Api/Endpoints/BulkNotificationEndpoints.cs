using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Application.Constants;
using BuildingBlocks.Web.Authorization;
using BuildingBlocks.Web.Extensions;
using BuildingBlocks.Web.Helpers;
using System.Security.Claims;
using Notification.Application.Features.Notifications.QueueBulkNotification;

namespace Notification.Api.Endpoints;

public class BulkNotificationEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/notifications/bulk")
            .WithTags("Bulk Notifications")
            .WithMetadata(new RequireAdminAttribute());

        group.MapPost("/", QueueBulkNotification)
            .WithName("QueueBulkNotification")
            .WithSummary("Queue bulk notifications to be sent via background worker")
            .WithDescription("Queues a bulk notification job for processing. Returns immediately with a correlation ID for tracking.");
    }

    private static async Task<IResult> QueueBulkNotification(
        ClaimsPrincipal user,
        ISender sender,
        [FromBody] QueueBulkNotificationRequest request,
        CancellationToken cancellationToken)
    {
        var userId = UserHelper.GetRequiredUserId(user);

        var command = new QueueBulkNotificationCommand(
            RequestedBy: userId,
            TemplateKey: request.TemplateKey,
            Title: request.Title,
            Message: request.Message,
            Channels: request.Channels,
            Recipients: request.Recipients.Select(r => new RecipientInfo(
                UserId: r.UserId,
                Email: r.Email,
                PhoneNumber: r.PhoneNumber,
                Parameters: r.Parameters
            )).ToList(),
            GlobalParameters: request.GlobalParameters,
            ScheduledAt: request.ScheduledAt,
            BatchSize: request.BatchSize ?? 100
        );

        var result = await sender.Send(command, cancellationToken);

        return result.ToApiResult(value =>
            Results.Accepted($"/api/v1/jobs/{value.CorrelationId}", value));
    }
}

public record QueueBulkNotificationRequest(
    string TemplateKey,
    string Title,
    string Message,
    List<NotificationChannel> Channels,
    List<RecipientRequest> Recipients,
    Dictionary<string, string>? GlobalParameters = null,
    DateTimeOffset? ScheduledAt = null,
    int? BatchSize = null);

public record RecipientRequest(
    Guid UserId,
    string Email,
    string? PhoneNumber = null,
    Dictionary<string, string>? Parameters = null);
