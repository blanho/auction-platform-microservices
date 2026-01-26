using Carter;
using Microsoft.AspNetCore.Http.HttpResults;
using BuildingBlocks.Application.Constants;
using BuildingBlocks.Web.Authorization;
using BuildingBlocks.Web.Helpers;
using Notification.Application.DTOs;
using Notification.Application.Interfaces;

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

    private static async Task<IResult> GetAllTemplates(
        int page,
        int pageSize,
        ITemplateService templateService,
        CancellationToken ct)
    {
        var result = await templateService.GetPagedAsync(page, pageSize, ct);
        if (!result.IsSuccess)
        {
            var problem = ProblemDetailsHelper.FromError(result.Error!);
            return Results.Problem(problem);
        }
        return Results.Ok(result.Value);
    }

    private static async Task<IResult> GetActiveTemplates(
        ITemplateService templateService,
        CancellationToken ct)
    {
        var result = await templateService.GetAllActiveAsync(ct);
        if (!result.IsSuccess)
        {
            var problem = ProblemDetailsHelper.FromError(result.Error!);
            return Results.Problem(problem);
        }
        return Results.Ok(result.Value);
    }

    private static async Task<IResult> GetTemplateByKey(
        string key,
        ITemplateService templateService,
        CancellationToken ct)
    {
        var result = await templateService.GetByKeyAsync(key, ct);
        if (!result.IsSuccess)
        {
            var problem = ProblemDetailsHelper.FromError(result.Error!);
            return Results.Problem(problem);
        }
        return Results.Ok(result.Value);
    }

    private static async Task<IResult> GetTemplateById(
        Guid id,
        ITemplateService templateService,
        CancellationToken ct)
    {
        var result = await templateService.GetByIdAsync(id, ct);
        if (!result.IsSuccess)
        {
            var problem = ProblemDetailsHelper.FromError(result.Error!);
            return Results.Problem(problem);
        }
        return Results.Ok(result.Value);
    }

    private static async Task<IResult> CreateTemplate(
        CreateTemplateDto dto,
        ITemplateService templateService,
        CancellationToken ct)
    {
        var result = await templateService.CreateAsync(dto, ct);
        if (!result.IsSuccess)
        {
            var problem = ProblemDetailsHelper.FromError(result.Error!);
            return Results.Problem(problem);
        }
        return Results.Created($"/api/v1/notifications/templates/{result.Value.Key}", result.Value);
    }

    private static async Task<IResult> UpdateTemplate(
        Guid id,
        UpdateTemplateDto dto,
        ITemplateService templateService,
        CancellationToken ct)
    {
        var result = await templateService.UpdateAsync(id, dto, ct);
        if (!result.IsSuccess)
        {
            var problem = ProblemDetailsHelper.FromError(result.Error!);
            return Results.Problem(problem);
        }
        return Results.Ok(result.Value);
    }

    private static async Task<IResult> DeleteTemplate(
        Guid id,
        ITemplateService templateService,
        CancellationToken ct)
    {
        var result = await templateService.DeleteAsync(id, ct);
        if (!result.IsSuccess)
        {
            var problem = ProblemDetailsHelper.FromError(result.Error!);
            return Results.Problem(problem);
        }
        return Results.NoContent();
    }

    private static async Task<Ok<bool>> CheckTemplateExists(
        string key,
        ITemplateService templateService,
        CancellationToken ct)
    {
        var exists = await templateService.ExistsAsync(key, ct);
        return TypedResults.Ok(exists);
    }
}
