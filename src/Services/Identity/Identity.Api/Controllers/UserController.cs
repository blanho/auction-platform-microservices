using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Web.Authorization;
using BuildingBlocks.Web.Helpers;
using Identity.Api.DTOs.Seller;
using Identity.Api.DTOs.TwoFactor;
using Identity.Api.DTOs.Users;
using Identity.Api.Errors;
using Identity.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IdentityUserHelper = Identity.Api.Helpers.UserHelper;

namespace Identity.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(AuthenticationSchemes = "Bearer")]
[Produces("application/json")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ITwoFactorService _twoFactorService;

    public UserController(
        IUserService userService,
        ITwoFactorService twoFactorService)
    {
        _userService = userService;
        _twoFactorService = twoFactorService;
    }

    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(PaginatedResult<AdminUserDto>), StatusCodes.Status200OK)]
    public async Task<IResult> GetUsers([FromQuery] GetUsersQuery query, CancellationToken cancellationToken = default)
    {
        var result = await _userService.GetUsersPagedAsync(query, cancellationToken);
        return Results.Ok(result);
    }

    [HttpGet("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(AdminUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IResult> GetUser(string id, CancellationToken cancellationToken = default)
    {
        var user = await _userService.GetUserByIdAsync(id, cancellationToken);
        return user == null
            ? Results.NotFound(ProblemDetailsHelper.NotFound("User", id))
            : Results.Ok(user);
    }

    [HttpGet("seller/status")]
    [ProducesResponseType(typeof(SellerStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IResult> GetSellerStatus(CancellationToken cancellationToken)
    {
        var validationResult = IdentityUserHelper.ValidateUserId(User);
        if (validationResult != null)
            return validationResult;

        var userId = IdentityUserHelper.GetUserId(User);
        var result = await _userService.GetSellerStatusAsync(userId, cancellationToken);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.Unauthorized();
    }

    [HttpPost("seller/apply")]
    [ProducesResponseType(typeof(SellerStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IResult> ApplyForSeller([FromBody] BecomeSellerRequest request, CancellationToken cancellationToken)
    {
        var validationResult = IdentityUserHelper.ValidateUserId(User);
        if (validationResult != null)
            return validationResult;

        var userId = IdentityUserHelper.GetUserId(User);
        var result = await _userService.ApplyForSellerAsync(userId, request.AcceptTerms, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Error == IdentityErrors.User.NotFound)
                return Results.Unauthorized();
            return Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
        }

        return Results.Ok(result.Value);
    }

    [HttpPost("{id}/suspend")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IResult> SuspendUser(string id, [FromBody] SuspendUserRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _userService.SuspendUserAsync(id, request.Reason, cancellationToken);
        return result.IsSuccess
            ? Results.Ok()
            : Results.NotFound(ProblemDetailsHelper.NotFound("User", id));
    }

    [HttpPost("{id}/unsuspend")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IResult> UnsuspendUser(string id, CancellationToken cancellationToken = default)
    {
        var result = await _userService.UnsuspendUserAsync(id, cancellationToken);
        return result.IsSuccess
            ? Results.Ok()
            : Results.NotFound(ProblemDetailsHelper.NotFound("User", id));
    }

    [HttpPost("{id}/activate")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IResult> ActivateUser(string id, CancellationToken cancellationToken = default)
    {
        var result = await _userService.ActivateUserAsync(id, cancellationToken);
        return result.IsSuccess
            ? Results.Ok()
            : Results.NotFound(ProblemDetailsHelper.NotFound("User", id));
    }

    [HttpPost("{id}/deactivate")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IResult> DeactivateUser(string id, CancellationToken cancellationToken = default)
    {
        var result = await _userService.DeactivateUserAsync(id, cancellationToken);
        return result.IsSuccess
            ? Results.Ok()
            : Results.NotFound(ProblemDetailsHelper.NotFound("User", id));
    }

    [HttpPut("{id}/roles")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IResult> UpdateUserRoles(string id, [FromBody] UpdateUserRolesRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _userService.UpdateUserRolesAsync(id, request.Roles, cancellationToken);
        return result.IsSuccess
            ? Results.Ok()
            : Results.NotFound(ProblemDetailsHelper.NotFound("User", id));
    }

    [HttpDelete("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IResult> DeleteUser(string id, CancellationToken cancellationToken = default)
    {
        var result = await _userService.DeleteUserAsync(id, cancellationToken);
        return result.IsSuccess
            ? Results.NoContent()
            : Results.NotFound(ProblemDetailsHelper.NotFound("User", id));
    }

    [HttpGet("stats")]
    [Authorize]
    [ProducesResponseType(typeof(AdminStatsResponse), StatusCodes.Status200OK)]
    public async Task<IResult> GetStats(CancellationToken cancellationToken = default)
    {
        var stats = await _userService.GetStatsAsync(cancellationToken);
        return Results.Ok(stats);
    }

    [HttpGet("{id}/2fa/status")]
    [Authorize]
    [ProducesResponseType(typeof(TwoFactorStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IResult> GetUser2FAStatus(string id, CancellationToken cancellationToken = default)
    {
        var result = await _twoFactorService.GetStatusByAdminAsync(id);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.NotFound(ProblemDetailsHelper.NotFound("User", id));
    }

    [HttpPost("{id}/2fa/reset")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IResult> Reset2FA(string id, CancellationToken cancellationToken = default)
    {
        var result = await _twoFactorService.ResetByAdminAsync(id);
        return result.IsSuccess
            ? Results.Ok()
            : Results.NotFound(ProblemDetailsHelper.NotFound("User", id));
    }

    [HttpPost("{id}/2fa/disable")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IResult> Disable2FA(string id, CancellationToken cancellationToken = default)
    {
        var result = await _twoFactorService.DisableByAdminAsync(id);

        if (!result.IsSuccess)
        {
            if (result.Error == IdentityErrors.User.NotFound)
                return Results.NotFound(ProblemDetailsHelper.NotFound("User", id));
            return Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
        }

        return Results.Ok();
    }
}
