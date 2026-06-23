using BuildingBlocks.Web.Authorization;
using BuildingBlocks.Web.Helpers;
using Identity.Application.DTOs.Auth;
using Identity.Application.DTOs.Profile;
using Identity.Application.Errors;
using Identity.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Api.Controllers;

[ApiController]
[Route("api/profile")]
[Authorize]
[Produces("application/json")]
public class ProfileController : ControllerBase
{
    private readonly MediatR.ISender _sender;

    public ProfileController(MediatR.ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IResult> GetProfile(CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserIdString();
        var result = await _sender.Send(new Identity.Application.Features.Profile.Queries.GetProfile.GetProfileQuery(userId), cancellationToken);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.NotFound(ProblemDetailsHelper.NotFound("Profile", userId));
    }

    [HttpPut]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IResult> UpdateProfile(
        [FromBody] UpdateProfileRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserIdString();
        var result = await _sender.Send(new Identity.Application.Features.Profile.Commands.UpdateProfile.UpdateProfileCommand(userId, request), cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Error == IdentityErrors.User.NotFound)
                return Results.NotFound(ProblemDetailsHelper.NotFound("Profile", userId));
            return Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
        }

        return Results.Ok(result.Value);
    }

    [HttpPost("change-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IResult> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserIdString();
        var result = await _sender.Send(new Identity.Application.Features.Profile.Commands.ChangePassword.ChangePasswordCommand(userId, request), cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Error == IdentityErrors.User.NotFound)
                return Results.NotFound(ProblemDetailsHelper.NotFound("User", userId));
            return Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
        }

        return Results.Ok();
    }

    [HttpPost("enable-2fa")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IResult> EnableTwoFactor(CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserIdString();
        var result = await _sender.Send(new Identity.Application.Features.Profile.Commands.EnableTwoFactor.EnableTwoFactorCommand(userId), cancellationToken);
        return result.IsSuccess
            ? Results.Ok()
            : Results.NotFound(ProblemDetailsHelper.NotFound("User", userId));
    }

    [HttpPost("disable-2fa")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IResult> DisableTwoFactor(CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserIdString();
        var result = await _sender.Send(new Identity.Application.Features.Profile.Commands.DisableTwoFactor.DisableTwoFactorCommand(userId), cancellationToken);
        return result.IsSuccess
            ? Results.Ok()
            : Results.NotFound(ProblemDetailsHelper.NotFound("User", userId));
    }
}
