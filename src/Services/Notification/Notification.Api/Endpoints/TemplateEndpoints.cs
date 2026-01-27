using Carter;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Constants;
using BuildingBlocks.Web.Authorization;
using Notification.Application.DTOs;
using Notification.Application.Features.Templates.GetTemplates;
using Notification.Application.Features.Templates.GetActiveTemplates;
using Notification.Application.Features.Templates.GetTemplateByKey;
using Notification.Application.Features.Templates.GetTemplateById;
using Notification.Application.Features.Templates.CreateTemplate;
using Notification.Application.Features.Templates.UpdateTemplate;
using Notification.Application.Features.Templates.DeleteTemplate;
using Notification.Application.Features.Templates.CheckTemplateExists;

namespace Notification.Api.Endpoints;

public class TemplateEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/notifications/templates")
            .WithTags("Notification Templates")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Notifications.ManageTemplates));

        group.MapGet("/", GetAllTemplates)
            .WithName("GetAllTemplates")
            .WithSummary("Get all notification templates (paged)");

        group.MapGet("/active", GetActiveTemplates)
            .WithName("GetActiveTemplates")
            .WithSummary("Get all active notification templates");

        group.MapGet("/{key}", GetTemplateByKey)
            .WithName("GetTemplateByKey")
            .WithSummary("Get a notification template by key");

        group.MapGet("/id/{id:guid}", GetTemplateById)
            .WithName("GetTemplateById")
            .WithSummary("Get a notification template by ID");

        group.MapPost("/", CreateTemplate)
            .WithName("CreateTemplate")
            .WithSummary("Create a new notification template");

        group.MapPut("/{id:guid}", UpdateTemplate)
            .WithName("UpdateTemplate")
            .WithSummary("Update an existing notification template");

        group.MapDelete("/{id:guid}", DeleteTemplate)
            .WithName("DeleteTemplate")
            .WithSummary("Delete a notification template");

        group.MapGet("/exists/{key}", CheckTemplateExists)
            .WithName("CheckTemplateExists")
            .WithSummary("Check if a template with the given key exists");
    }

    private static async Task<Ok<PaginatedResult<TemplateDto>>> GetAllTemplates(
        int page,
        int pageSize,
        ISender sender,
        CancellationToken ct)
    {
        var query = new GetTemplatesQuery(page, pageSize);
        var result = await sender.Send(query, ct);
        return TypedResults.Ok(result.Value);
    }

    private static async Task<Ok<List<TemplateDto>>> GetActiveTemplates(
        ISender sender,
        CancellationToken ct)
    {
        var query = new GetActiveTemplatesQuery();
        var result = await sender.Send(query, ct);
        return TypedResults.Ok(result.Value);
    }

    private static async Task<Ok<TemplateDto>> GetTemplateByKey(
        string key,
        ISender sender,
        CancellationToken ct)
    {
        var query = new GetTemplateByKeyQuery(key);
        var result = await sender.Send(query, ct);
        return TypedResults.Ok(result.Value);
    }

    private static async Task<Ok<TemplateDto>> GetTemplateById(
        Guid id,
        ISender sender,
        CancellationToken ct)
    {
        var query = new GetTemplateByIdQuery(id);
        var result = await sender.Send(query, ct);
        return TypedResults.Ok(result.Value);
    }

    private static async Task<Created<TemplateDto>> CreateTemplate(
        CreateTemplateDto dto,
        ISender sender,
        CancellationToken ct)
    {
        var command = new CreateTemplateCommand(
            dto.Key,
            dto.Name,
            dto.Subject,
            dto.Body,
            dto.Description,
            dto.SmsBody,
            dto.PushTitle,
            dto.PushBody);
        var result = await sender.Send(command, ct);
        return TypedResults.Created($"/api/v1/notifications/templates/{result.Value.Key}", result.Value);
    }

    private static async Task<Ok<TemplateDto>> UpdateTemplate(
        Guid id,
        UpdateTemplateDto dto,
        ISender sender,
        CancellationToken ct)
    {
        var command = new UpdateTemplateCommand(
            id,
            dto.Name,
            dto.Subject,
            dto.Body,
            dto.Description,
            dto.SmsBody,
            dto.PushTitle,
            dto.PushBody,
            dto.IsActive);
        var result = await sender.Send(command, ct);
        return TypedResults.Ok(result.Value);
    }

    private static async Task<NoContent> DeleteTemplate(
        Guid id,
        ISender sender,
        CancellationToken ct)
    {
        var command = new DeleteTemplateCommand(id);
        await sender.Send(command, ct);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<bool>> CheckTemplateExists(
        string key,
        ISender sender,
        CancellationToken ct)
    {
        var query = new CheckTemplateExistsQuery(key);
        var result = await sender.Send(query, ct);
        return TypedResults.Ok(result.Value);
    }
}
