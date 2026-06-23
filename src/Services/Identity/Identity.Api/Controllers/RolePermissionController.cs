using BuildingBlocks.Web.Authorization;
using BuildingBlocks.Web.Helpers;
using Identity.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Api.Controllers;

[ApiController]
[Route("api/roles")]
[Authorize(AuthenticationSchemes = "Bearer")]
[Produces("application/json")]
public class RolePermissionController : ControllerBase
{
    private readonly MediatR.ISender _sender;

    public RolePermissionController(MediatR.ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [RequirePermission(Permissions.Users.ManageRoles)]
    [ProducesResponseType(typeof(IReadOnlyList<RoleDto>), StatusCodes.Status200OK)]
    public async Task<IResult> GetAllRoles(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new Identity.Application.Features.RolePermissions.Queries.GetAllRoles.GetAllRolesQuery(), cancellationToken);
        return Results.Ok(result.Value);
    }

    [HttpGet("{roleId:guid}")]
    [RequirePermission(Permissions.Users.ManageRoles)]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IResult> GetRole(Guid roleId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new Identity.Application.Features.RolePermissions.Queries.GetRoleById.GetRoleByIdQuery(roleId), cancellationToken);
        return result.Value is null
            ? Results.NotFound(ProblemDetailsHelper.NotFound("Role", roleId.ToString()))
            : Results.Ok(result.Value);
    }

    [HttpGet("{roleId:guid}/permissions")]
    [RequirePermission(Permissions.Users.ManageRoles)]
    [ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IResult> GetRolePermissions(Guid roleId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new Identity.Application.Features.RolePermissions.Queries.GetPermissionsForRole.GetPermissionsForRoleQuery(roleId), cancellationToken);
        return Results.Ok(result.Value);
    }

    [HttpGet("permissions/definitions")]
    [RequirePermission(Permissions.Users.ManageRoles)]
    [ProducesResponseType(typeof(IReadOnlyList<PermissionDefinition>), StatusCodes.Status200OK)]
    public async Task<IResult> GetAllPermissionDefinitions(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new Identity.Application.Features.RolePermissions.Queries.GetAllPermissionDefinitions.GetAllPermissionDefinitionsQuery(), cancellationToken);
        return Results.Ok(result.Value);
    }

    [HttpPost("{roleId:guid}/permissions/{permission}")]
    [RequirePermission(Permissions.Users.ManageRoles)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IResult> GrantPermission(Guid roleId, string permission, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new Identity.Application.Features.RolePermissions.Commands.GrantPermission.GrantPermissionCommand(roleId, permission), cancellationToken);
        return result.Value
            ? Results.NoContent()
            : Results.NotFound(ProblemDetailsHelper.NotFound("Role", roleId.ToString()));
    }

    [HttpDelete("{roleId:guid}/permissions/{permission}")]
    [RequirePermission(Permissions.Users.ManageRoles)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> RevokePermission(Guid roleId, string permission, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new Identity.Application.Features.RolePermissions.Commands.RevokePermission.RevokePermissionCommand(roleId, permission), cancellationToken);
        return result.Value
            ? Results.NoContent()
            : Results.NotFound(ProblemDetailsHelper.NotFound("Role", roleId.ToString()));
    }

    [HttpPut("{roleId:guid}/permissions")]
    [RequirePermission(Permissions.Users.ManageRoles)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IResult> SetPermissions(Guid roleId, [FromBody] SetPermissionsRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new Identity.Application.Features.RolePermissions.Commands.SetPermissions.SetPermissionsCommand(roleId, request.Permissions), cancellationToken);
        return result.Value
            ? Results.NoContent()
            : Results.NotFound(ProblemDetailsHelper.NotFound("Role", roleId.ToString()));
    }

    [HttpPost("{roleId:guid}/permissions/toggle")]
    [RequirePermission(Permissions.Users.ManageRoles)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IResult> TogglePermission(Guid roleId, [FromBody] TogglePermissionRequest request, CancellationToken cancellationToken)
    {
        bool result;
        if (request.Enabled)
        {
            var r = await _sender.Send(new Identity.Application.Features.RolePermissions.Commands.GrantPermission.GrantPermissionCommand(roleId, request.Permission), cancellationToken);
            result = r.Value;
        }
        else
        {
            var r = await _sender.Send(new Identity.Application.Features.RolePermissions.Commands.RevokePermission.RevokePermissionCommand(roleId, request.Permission), cancellationToken);
            result = r.Value;
        }

        return result
            ? Results.NoContent()
            : Results.NotFound(ProblemDetailsHelper.NotFound("Role", roleId.ToString()));
    }
}

public record SetPermissionsRequest(IEnumerable<string> Permissions);
public record TogglePermissionRequest(string Permission, bool Enabled);
