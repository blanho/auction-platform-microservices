using BuildingBlocks.Web.Authorization;
using BuildingBlocks.Web.Helpers;
using Identity.Application.DTOs.Seller;
using Identity.Application.DTOs.TwoFactor;
using Identity.Application.DTOs.Users;
using Identity.Application.Errors;
using Identity.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(AuthenticationSchemes = "Bearer")]
[Produces("application/json")]
public class UserController : ControllerBase
{
    private readonly MediatR.ISender _sender;
    public UserController(MediatR.ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(PaginatedResult<AdminUserDto>), StatusCodes.Status200OK)]
    public async Task<IResult> GetUsers([FromQuery] Identity.Application.DTOs.Users.GetUsersQuery query, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new Identity.Application.Features.Users.Queries.GetUsers.GetUsersListQuery(query), cancellationToken);
        return Results.Ok(result.Value);
    }

    [HttpGet("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(AdminUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IResult> GetUser(string id, CancellationToken cancellationToken = default)
    {
        var user = await _sender.Send(new Identity.Application.Features.Users.Queries.GetUserById.GetUserByIdQuery(id), cancellationToken);
        return user == null
            ? Results.NotFound(ProblemDetailsHelper.NotFound("User", id))
            : Results.Ok(user);
    }

    [HttpGet("seller/status")]
    [ProducesResponseType(typeof(SellerStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IResult> GetSellerStatus(CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserIdString();
        var result = await _sender.Send(new Identity.Application.Features.Users.Queries.GetSellerStatus.GetSellerStatusQuery(userId), cancellationToken);
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
        var userId = User.GetRequiredUserIdString();
        var result = await _sender.Send(new Identity.Application.Features.Users.Commands.ApplyForSeller.ApplyForSellerCommand(userId, request.AcceptTerms), cancellationToken);

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
        var result = await _sender.Send(new Identity.Application.Features.Users.Commands.SuspendUser.SuspendUserCommand(id, request.Reason), cancellationToken);
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
        var result = await _sender.Send(new Identity.Application.Features.Users.Commands.UnsuspendUser.UnsuspendUserCommand(id), cancellationToken);
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
        var result = await _sender.Send(new Identity.Application.Features.Users.Commands.ActivateUser.ActivateUserCommand(id), cancellationToken);
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
        var result = await _sender.Send(new Identity.Application.Features.Users.Commands.DeactivateUser.DeactivateUserCommand(id), cancellationToken);
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
        var result = await _sender.Send(new Identity.Application.Features.Users.Commands.UpdateUserRoles.UpdateUserRolesCommand(id, request.Roles), cancellationToken);
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
        var result = await _sender.Send(new Identity.Application.Features.Users.Commands.DeleteUser.DeleteUserCommand(id), cancellationToken);
        return result.IsSuccess
            ? Results.NoContent()
            : Results.NotFound(ProblemDetailsHelper.NotFound("User", id));
    }

    [HttpGet("stats")]
    [Authorize]
    [ProducesResponseType(typeof(AdminStatsResponse), StatusCodes.Status200OK)]
    public async Task<IResult> GetStats(CancellationToken cancellationToken = default)
    {
        var statsResult = await _sender.Send(new Identity.Application.Features.Users.Queries.GetAdminStats.GetAdminStatsQuery(), cancellationToken);
        return Results.Ok(statsResult.Value);
    }

    [HttpGet("{id}/2fa/status")]
    [Authorize]
    [ProducesResponseType(typeof(TwoFactorStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IResult> GetUser2FAStatus(string id, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new Identity.Application.Features.TwoFactor.Queries.GetStatusByAdmin.GetStatusByAdminQuery(id), cancellationToken);
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
        var result = await _sender.Send(new Identity.Application.Features.TwoFactor.Commands.ResetByAdmin.ResetByAdminCommand(id), cancellationToken);
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
        var result = await _sender.Send(new Identity.Application.Features.TwoFactor.Commands.DisableByAdmin.DisableByAdminCommand(id), cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Error == IdentityErrors.User.NotFound)
                return Results.NotFound(ProblemDetailsHelper.NotFound("User", id));
            return Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
        }

        return Results.Ok();
    }
}
