using BuildingBlocks.Web.Authorization;
using BuildingBlocks.Web.Helpers;
using Identity.Api.DTOs.Auth;
using Identity.Api.DTOs.Profile;
using Identity.Api.Errors;
using Identity.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Api.Controllers;

[ApiController]
[Route("api/profile")]
[Authorize]
[Produces("application/json")]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;

    public ProfileController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IResult> GetProfile(CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserIdString();
        var result = await _profileService.GetProfileAsync(userId, cancellationToken);
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
        var result = await _profileService.UpdateProfileAsync(userId, request, cancellationToken);

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
        var result = await _profileService.ChangePasswordAsync(userId, request, cancellationToken);

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
        var result = await _profileService.EnableTwoFactorAsync(userId, cancellationToken);
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
        var result = await _profileService.DisableTwoFactorAsync(userId, cancellationToken);
        return result.IsSuccess
            ? Results.Ok()
            : Results.NotFound(ProblemDetailsHelper.NotFound("User", userId));
    }
}
