using Carter;
using Microsoft.AspNetCore.Http.HttpResults;
using Analytics.Api.Models;
using Analytics.Api.Interfaces;
using Analytics.Api.Enums;
using BuildingBlocks.Web.Authorization;

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
            .Produces<List<PlatformSettingDto>>();

        group.MapGet("/{id:guid}", GetSetting)
            .WithName("GetSetting")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Users.ManageSettings))
            .Produces<PlatformSettingDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/key/{key}", GetSettingByKey)
            .WithName("GetSettingByKey")
            .AllowAnonymous()
            .Produces<PlatformSettingDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("", CreateSetting)
            .WithName("CreateSetting")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Users.ManageSettings))
            .Produces<PlatformSettingDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPut("/{id:guid}", UpdateSetting)
            .WithName("UpdateSetting")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Users.ManageSettings))
            .Produces<PlatformSettingDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", DeleteSetting)
            .WithName("DeleteSetting")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Users.ManageSettings))
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/bulk", BulkUpdateSettings)
            .WithName("BulkUpdateSettings")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Users.ManageSettings))
            .Produces(StatusCodes.Status200OK);
    }

    private static async Task<Ok<List<PlatformSettingDto>>> GetSettings(
        SettingCategory? category,
        IPlatformSettingService settingService,
        CancellationToken cancellationToken)
    {
        var settings = await settingService.GetSettingsAsync(category, cancellationToken);
        return TypedResults.Ok(settings);
    }

    private static async Task<Results<Ok<PlatformSettingDto>, NotFound>> GetSetting(
        Guid id,
        IPlatformSettingService settingService,
        CancellationToken cancellationToken)
    {
        var setting = await settingService.GetSettingByIdAsync(id, cancellationToken);
        return setting is not null
            ? TypedResults.Ok(setting)
            : TypedResults.NotFound();
    }

    private static async Task<Results<Ok<PlatformSettingDto>, NotFound>> GetSettingByKey(
        string key,
        IPlatformSettingService settingService,
        CancellationToken cancellationToken)
    {
        var setting = await settingService.GetSettingByKeyAsync(key, cancellationToken);
        return setting is not null
            ? TypedResults.Ok(setting)
            : TypedResults.NotFound();
    }

    private static async Task<Created<PlatformSettingDto>> CreateSetting(
        CreateSettingDto dto,
        HttpContext httpContext,
        IPlatformSettingService settingService,
        CancellationToken cancellationToken)
    {
        var username = httpContext.User.Identity?.Name;
        var setting = await settingService.CreateSettingAsync(dto, username, cancellationToken);
        return TypedResults.Created($"/api/v1/settings/{setting.Id}", setting);
    }

    private static async Task<Ok<PlatformSettingDto>> UpdateSetting(
        Guid id,
        UpdateSettingDto dto,
        HttpContext httpContext,
        IPlatformSettingService settingService,
        CancellationToken cancellationToken)
    {
        var username = httpContext.User.Identity?.Name;
        var setting = await settingService.UpdateSettingAsync(id, dto, username, cancellationToken);
        return TypedResults.Ok(setting);
    }

    private static async Task<NoContent> DeleteSetting(
        Guid id,
        IPlatformSettingService settingService,
        CancellationToken cancellationToken)
    {
        await settingService.DeleteSettingAsync(id, cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<Ok> BulkUpdateSettings(
        BulkUpdateSettingsDto dto,
        HttpContext httpContext,
        IPlatformSettingService settingService,
        CancellationToken cancellationToken)
    {
        var username = httpContext.User.Identity?.Name;
        await settingService.BulkUpdateSettingsAsync(dto.Settings, username, cancellationToken);
        return TypedResults.Ok();
    }
}

public class BulkUpdateSettingsDto
{
    public List<SettingKeyValue> Settings { get; set; } = new();
}
