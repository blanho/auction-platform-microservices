using BuildingBlocks.Web.Authorization;
using BuildingBlocks.Web.Helpers;
using Identity.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Api.Controllers;

[ApiController]
[Route("api/roles")]
[Authorize(AuthenticationSchemes = "Bearer")]
[Produces("application/json")]
public class RolePermissionController : ControllerBase
{
    private readonly IRolePermissionService _rolePermissionService;

    public RolePermissionController(IRolePermissionService rolePermissionService)
    {
        _rolePermissionService = rolePermissionService;
    }

    [HttpGet]
    [RequirePermission(Permissions.Users.ManageRoles)]
    [ProducesResponseType(typeof(IReadOnlyList<RoleDto>), StatusCodes.Status200OK)]
    public async Task<IResult> GetAllRoles(CancellationToken cancellationToken)
    {
        var roles = await _rolePermissionService.GetAllRolesAsync(cancellationToken);
        return Results.Ok(roles);
    }

    [HttpGet("{roleId:guid}")]
    [RequirePermission(Permissions.Users.ManageRoles)]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IResult> GetRole(Guid roleId, CancellationToken cancellationToken)
    {
        var role = await _rolePermissionService.GetRoleByIdAsync(roleId, cancellationToken);
        return role is null
            ? Results.NotFound(ProblemDetailsHelper.NotFound("Role", roleId.ToString()))
            : Results.Ok(role);
    }

    [HttpGet("{roleId:guid}/permissions")]
    [RequirePermission(Permissions.Users.ManageRoles)]
    [ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IResult> GetRolePermissions(Guid roleId, CancellationToken cancellationToken)
    {
        var permissions = await _rolePermissionService.GetPermissionsForRoleAsync(roleId, cancellationToken);
        return Results.Ok(permissions);
    }

    [HttpGet("permissions/definitions")]
    [RequirePermission(Permissions.Users.ManageRoles)]
    [ProducesResponseType(typeof(IReadOnlyList<PermissionDefinition>), StatusCodes.Status200OK)]
    public async Task<IResult> GetAllPermissionDefinitions()
    {
        var definitions = await _rolePermissionService.GetAllPermissionDefinitionsAsync();
        return Results.Ok(definitions);
    }

    [HttpPost("{roleId:guid}/permissions/{permission}")]
    [RequirePermission(Permissions.Users.ManageRoles)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IResult> GrantPermission(Guid roleId, string permission, CancellationToken cancellationToken)
    {
        var result = await _rolePermissionService.GrantPermissionAsync(roleId, permission, cancellationToken);
        return result
            ? Results.NoContent()
            : Results.NotFound(ProblemDetailsHelper.NotFound("Role", roleId.ToString()));
    }

    [HttpDelete("{roleId:guid}/permissions/{permission}")]
    [RequirePermission(Permissions.Users.ManageRoles)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> RevokePermission(Guid roleId, string permission, CancellationToken cancellationToken)
    {
        await _rolePermissionService.RevokePermissionAsync(roleId, permission, cancellationToken);
        return Results.NoContent();
    }

    [HttpPut("{roleId:guid}/permissions")]
    [RequirePermission(Permissions.Users.ManageRoles)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IResult> SetPermissions(Guid roleId, [FromBody] SetPermissionsRequest request, CancellationToken cancellationToken)
    {
        var result = await _rolePermissionService.SetPermissionsAsync(roleId, request.Permissions, cancellationToken);
        return result
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
            result = await _rolePermissionService.GrantPermissionAsync(roleId, request.Permission, cancellationToken);
        }
        else
        {
            result = await _rolePermissionService.RevokePermissionAsync(roleId, request.Permission, cancellationToken);
        }

        return result
            ? Results.NoContent()
            : Results.NotFound(ProblemDetailsHelper.NotFound("Role", roleId.ToString()));
    }
}

public record SetPermissionsRequest(IEnumerable<string> Permissions);
public record TogglePermissionRequest(string Permission, bool Enabled);
