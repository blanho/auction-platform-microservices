using Carter;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using BuildingBlocks.Web.Authorization;
using BuildingBlocks.Web.Helpers;
using Identity.Application.Features.RolePermissions.Queries.GetAllRoles;
using Identity.Application.Features.RolePermissions.Queries.GetRoleById;
using Identity.Application.Features.RolePermissions.Queries.GetPermissionsForRole;
using Identity.Application.Features.RolePermissions.Queries.GetPermissionsForRoles;
using Identity.Application.Features.RolePermissions.Queries.GetRoleByName;
using Identity.Application.Features.RolePermissions.Queries.GetAllPermissionDefinitions;
using Identity.Application.Features.RolePermissions.Commands.SetPermissions;
using Identity.Application.Features.RolePermissions.Commands.GrantPermission;
using Identity.Application.Features.RolePermissions.Commands.RevokePermission;
using Identity.Domain.Entities;

namespace Identity.Api.Endpoints.RolePermissions;

public class RolePermissionEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/roles")
            .WithTags("Roles & Permissions")
            .RequireAuthorization();

        group.MapGet("", GetRoles)
            .WithName("GetRoles")
            .Produces<IEnumerable<AppRole>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", GetRole)
            .WithName("GetRole")
            .Produces<AppRole>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapGet("/{roleId:guid}/permissions", GetPermissionsForRole)
            .WithName("GetPermissionsForRole")
            .Produces<IEnumerable<string>>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapGet("/permissions", GetPermissionsForRoles)
            .WithName("GetPermissionsForRoles")
            .Produces<IEnumerable<string>>(StatusCodes.Status200OK);

        group.MapGet("/name/{roleName}", GetRoleByName)
            .WithName("GetRoleByName")
            .Produces<AppRole>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapGet("/permissions/definitions", GetAllPermissionDefinitions)
            .WithName("GetAllPermissionDefinitions")
            .Produces<IEnumerable<string>>(StatusCodes.Status200OK);

        group.MapPost("/{roleId:guid}/permissions/set", SetPermissions)
            .WithName("SetPermissions")
            .RequireAuthorization($"Permission:{Permissions.Users.ManageRoles}")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapPost("/{roleId:guid}/permissions/toggle", TogglePermission)
            .WithName("TogglePermission")
            .RequireAuthorization($"Permission:{Permissions.Users.ManageRoles}")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GetRoles(ISender sender, CancellationToken cancellationToken)
    {
        var roles = await sender.Send(new GetAllRolesQuery(), cancellationToken);
        return Results.Ok(roles);
    }

    private static async Task<IResult> GetRole(Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var role = await sender.Send(new GetRoleByIdQuery(id), cancellationToken);
        return role == null
            ? Results.NotFound(ProblemDetailsHelper.NotFound("Role", id.ToString()))
            : Results.Ok(role);
    }

    private static async Task<IResult> GetPermissionsForRole(Guid roleId, ISender sender, CancellationToken cancellationToken)
    {
        var permissions = await sender.Send(new GetPermissionsForRoleQuery(roleId), cancellationToken);
        return permissions == null
            ? Results.NotFound(ProblemDetailsHelper.NotFound("Role", roleId.ToString()))
            : Results.Ok(permissions);
    }

    private static async Task<IResult> GetPermissionsForRoles([FromQuery(Name = "roleNames")] string[] roleNames, ISender sender, CancellationToken cancellationToken)
    {
        var permissions = await sender.Send(new GetPermissionsForRolesQuery(roleNames), cancellationToken);
        return Results.Ok(permissions);
    }

    private static async Task<IResult> GetRoleByName(string roleName, ISender sender, CancellationToken cancellationToken)
    {
        var role = await sender.Send(new GetRoleByNameQuery(roleName), cancellationToken);
        return role == null
            ? Results.NotFound(ProblemDetailsHelper.NotFound("Role", roleName))
            : Results.Ok(role);
    }

    private static async Task<IResult> GetAllPermissionDefinitions(ISender sender, CancellationToken cancellationToken)
    {
        var permissions = await sender.Send(new GetAllPermissionDefinitionsQuery(), cancellationToken);
        return Results.Ok(permissions);
    }

    private static async Task<IResult> SetPermissions(Guid roleId, [FromBody] SetPermissionsRequest request, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SetPermissionsCommand(roleId, request.Permissions), cancellationToken);
        return result.Value
            ? Results.NoContent()
            : Results.NotFound(ProblemDetailsHelper.NotFound("Role", roleId.ToString()));
    }

    private static async Task<IResult> TogglePermission(Guid roleId, [FromBody] TogglePermissionRequest request, ISender sender, CancellationToken cancellationToken)
    {
        bool result;
        if (request.Enabled)
        {
            var r = await sender.Send(new GrantPermissionCommand(roleId, request.Permission), cancellationToken);
            result = r.Value;
        }
        else
        {
            var r = await sender.Send(new RevokePermissionCommand(roleId, request.Permission), cancellationToken);
            result = r.Value;
        }

        return result
            ? Results.NoContent()
            : Results.NotFound(ProblemDetailsHelper.NotFound("Role", roleId.ToString()));
    }
}

public record SetPermissionsRequest(IEnumerable<string> Permissions);
public record TogglePermissionRequest(string Permission, bool Enabled);
