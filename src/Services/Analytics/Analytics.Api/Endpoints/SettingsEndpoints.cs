using Carter;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Analytics.Api.Models;
using Analytics.Api.Interfaces;
using Analytics.Api.Enums;
using BuildingBlocks.Web.Authorization;
using BuildingBlocks.Web.Helpers;

namespace Analytics.Api.Endpoints;

public class SettingsEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/settings")
            .WithTags("Settings");

        group.MapGet("", GetSettings)
            .WithName("GetSettings")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Users.ManageSettings))
            .Produces<List<PlatformSettingDto>>()
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapGet("/{id:guid}", GetSetting)
            .WithName("GetSetting")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Users.ManageSettings))
            .Produces<PlatformSettingDto>()
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapGet("/key/{key}", GetSettingByKey)
            .WithName("GetSettingByKey")
            .AllowAnonymous()
            .Produces<PlatformSettingDto>()
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapPost("", CreateSetting)
            .WithName("CreateSetting")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Users.ManageSettings))
            .Produces<PlatformSettingDto>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

        group.MapPut("/{id:guid}", UpdateSetting)
            .WithName("UpdateSetting")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Users.ManageSettings))
            .Produces<PlatformSettingDto>()
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", DeleteSetting)
            .WithName("DeleteSetting")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Users.ManageSettings))
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapPut("/bulk", BulkUpdateSettings)
            .WithName("BulkUpdateSettings")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Users.ManageSettings))
            .Produces(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> GetSettings(
        SettingCategory? category,
        IPlatformSettingService settingService,
        CancellationToken cancellationToken)
    {
        var result = await settingService.GetSettingsAsync(category, cancellationToken);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }

    private static async Task<IResult> GetSetting(
        Guid id,
        IPlatformSettingService settingService,
        CancellationToken cancellationToken)
    {
        var result = await settingService.GetSettingByIdAsync(id, cancellationToken);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.NotFound(ProblemDetailsHelper.NotFound("Setting", id));
    }

    private static async Task<IResult> GetSettingByKey(
        string key,
        IPlatformSettingService settingService,
        CancellationToken cancellationToken)
    {
        var result = await settingService.GetSettingByKeyAsync(key, cancellationToken);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.NotFound(ProblemDetailsHelper.FromError(result.Error!));
    }

    private static async Task<IResult> CreateSetting(
        CreateSettingDto dto,
        HttpContext httpContext,
        IPlatformSettingService settingService,
        CancellationToken cancellationToken)
    {
        var username = UserHelper.GetUsername(httpContext.User);
        var result = await settingService.CreateSettingAsync(dto, username, cancellationToken);
        
        return result.IsSuccess
            ? Results.Created($"/api/v1/settings/{result.Value!.Id}", result.Value)
            : Results.Conflict(ProblemDetailsHelper.FromError(result.Error!));
    }

    private static async Task<IResult> UpdateSetting(
        Guid id,
        UpdateSettingDto dto,
        HttpContext httpContext,
        IPlatformSettingService settingService,
        CancellationToken cancellationToken)
    {
        var username = UserHelper.GetUsername(httpContext.User);
        var result = await settingService.UpdateSettingAsync(id, dto, username, cancellationToken);
        
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.NotFound(ProblemDetailsHelper.NotFound("Setting", id));
    }

    private static async Task<IResult> DeleteSetting(
        Guid id,
        IPlatformSettingService settingService,
        CancellationToken cancellationToken)
    {
        var result = await settingService.DeleteSettingAsync(id, cancellationToken);
        return result.IsSuccess
            ? Results.NoContent()
            : Results.NotFound(ProblemDetailsHelper.NotFound("Setting", id));
    }

    private static async Task<IResult> BulkUpdateSettings(
        BulkUpdateSettingsDto dto,
        HttpContext httpContext,
        IPlatformSettingService settingService,
        CancellationToken cancellationToken)
    {
        var username = UserHelper.GetUsername(httpContext.User);
        var result = await settingService.BulkUpdateSettingsAsync(dto.Settings, username, cancellationToken);
        
        return result.IsSuccess
            ? Results.Ok()
            : Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }
}

public class BulkUpdateSettingsDto
{
    public List<SettingKeyValue> Settings { get; set; } = new();
}
