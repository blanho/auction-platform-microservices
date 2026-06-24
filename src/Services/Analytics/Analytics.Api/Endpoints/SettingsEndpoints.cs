using Carter;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Analytics.Application.DTOs;
using Analytics.Application.Features.PlatformSettings;
using Analytics.Domain.Enums;
using BuildingBlocks.Web.Authorization;
using BuildingBlocks.Web.Extensions;
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
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Users.ManageSettings))
            .Produces<PlatformSettingDto>()
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapPost("", CreateSetting)
            .WithName("CreateSetting")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Users.ManageSettings))
            .WithValidation<CreateSettingDto>()
            .Produces<PlatformSettingDto>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

        group.MapPut("/{id:guid}", UpdateSetting)
            .WithName("UpdateSetting")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Users.ManageSettings))
            .WithValidation<UpdateSettingDto>()
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
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetSettingsQuery(category), cancellationToken);
        return result.ToOkResult();
    }

    private static async Task<IResult> GetSetting(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetSettingByIdQuery(id), cancellationToken);
        return result.ToOkResult();
    }

    private static async Task<IResult> GetSettingByKey(
        string key,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetSettingByKeyQuery(key), cancellationToken);
        return result.ToOkResult();
    }

    private static async Task<IResult> CreateSetting(
        CreateSettingDto dto,
        HttpContext httpContext,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var username = UserHelper.GetUsername(httpContext.User);
        var result = await sender.Send(new CreateSettingCommand(dto, username), cancellationToken);
        return result.ToApiResult(value => Results.Created($"/api/v1/settings/{value!.Id}", value));
    }

    private static async Task<IResult> UpdateSetting(
        Guid id,
        UpdateSettingDto dto,
        HttpContext httpContext,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var username = UserHelper.GetUsername(httpContext.User);
        var result = await sender.Send(new UpdateSettingCommand(id, dto, username), cancellationToken);
        return result.ToOkResult();
    }

    private static async Task<IResult> DeleteSetting(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteSettingCommand(id), cancellationToken);
        return result.ToNoContentResult();
    }

    private static async Task<IResult> BulkUpdateSettings(
        BulkUpdateSettingsDto dto,
        HttpContext httpContext,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var username = UserHelper.GetUsername(httpContext.User);
        var result = await sender.Send(new BulkUpdateSettingsCommand(dto.Settings, username), cancellationToken);
        return result.ToNoContentResult();
    }
}

public class BulkUpdateSettingsDto
{
    public List<SettingKeyValue> Settings { get; set; } = new();
}
