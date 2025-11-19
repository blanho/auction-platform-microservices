using BuildingBlocks.Web.Authorization;
using BuildingBlocks.Web.Helpers;
using Identity.Api.DTOs.TwoFactor;
using Identity.Api.Errors;
using Identity.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IdentityUserHelper = Identity.Api.Helpers.UserHelper;

namespace Identity.Api.Controllers;

[ApiController]
[Route("api/auth/2fa")]
[Authorize]
[Produces("application/json")]
public class TwoFactorController : ControllerBase
{
    private readonly ITwoFactorService _twoFactorService;

    public TwoFactorController(ITwoFactorService twoFactorService)
    {
        _twoFactorService = twoFactorService;
    }

    [HttpGet("status")]
    [ProducesResponseType(typeof(TwoFactorStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IResult> GetStatus(CancellationToken cancellationToken)
    {
        var validationResult = IdentityUserHelper.ValidateUserId(User);
        if (validationResult != null)
            return validationResult;

        var userId = IdentityUserHelper.GetUserId(User);
        var result = await _twoFactorService.GetStatusAsync(userId);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.NotFound(ProblemDetailsHelper.NotFound("User", userId));
    }

    [HttpPost("setup")]
    [ProducesResponseType(typeof(TwoFactorSetupResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IResult> SetupAuthenticator(CancellationToken cancellationToken)
    {
        var validationResult = IdentityUserHelper.ValidateUserId(User);
        if (validationResult != null)
            return validationResult;

        var userId = IdentityUserHelper.GetUserId(User);
        var result = await _twoFactorService.SetupAuthenticatorAsync(userId);

        if (!result.IsSuccess)
        {
            if (result.Error == IdentityErrors.User.NotFound)
                return Results.NotFound(ProblemDetailsHelper.NotFound("User", userId));
            return Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
        }

        return Results.Ok(result.Value);
    }

    [HttpPost("enable")]
    [ProducesResponseType(typeof(RecoveryCodesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IResult> EnableAuthenticator([FromBody] Enable2FARequest request, CancellationToken cancellationToken)
    {
        var validationResult = IdentityUserHelper.ValidateUserId(User);
        if (validationResult != null)
            return validationResult;

        var userId = IdentityUserHelper.GetUserId(User);
        var result = await _twoFactorService.EnableAuthenticatorAsync(userId, request.Code);

        if (!result.IsSuccess)
        {
            if (result.Error == IdentityErrors.User.NotFound)
                return Results.NotFound(ProblemDetailsHelper.NotFound("User", userId));
            return Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
        }

        return Results.Ok(result.Value);
    }

    [HttpPost("disable")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IResult> DisableAuthenticator([FromBody] Disable2FARequest request, CancellationToken cancellationToken)
    {
        var validationResult = IdentityUserHelper.ValidateUserId(User);
        if (validationResult != null)
            return validationResult;

        var userId = IdentityUserHelper.GetUserId(User);
        var result = await _twoFactorService.DisableAuthenticatorAsync(userId, request.Password);

        if (!result.IsSuccess)
        {
            if (result.Error == IdentityErrors.User.NotFound)
                return Results.NotFound(ProblemDetailsHelper.NotFound("User", userId));
            return Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
        }

        return Results.Ok();
    }

    [HttpPost("verify")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IResult> VerifyCode([FromBody] Verify2FARequest request, CancellationToken cancellationToken)
    {
        var result = await _twoFactorService.VerifyCodeAsync(request.Code, request.RememberDevice);
        return result.IsSuccess
            ? Results.Ok()
            : Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }

    [HttpPost("recovery")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IResult> UseRecoveryCode([FromBody] UseRecoveryCodeRequest request, CancellationToken cancellationToken)
    {
        var result = await _twoFactorService.UseRecoveryCodeAsync(request.RecoveryCode);
        return result.IsSuccess
            ? Results.Ok()
            : Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }

    [HttpPost("generate-codes")]
    [ProducesResponseType(typeof(RecoveryCodesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IResult> GenerateRecoveryCodes(CancellationToken cancellationToken)
    {
        var validationResult = IdentityUserHelper.ValidateUserId(User);
        if (validationResult != null)
            return validationResult;

        var userId = IdentityUserHelper.GetUserId(User);
        var result = await _twoFactorService.GenerateRecoveryCodesAsync(userId);

        if (!result.IsSuccess)
        {
            if (result.Error == IdentityErrors.User.NotFound)
                return Results.NotFound(ProblemDetailsHelper.NotFound("User", userId));
            return Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
        }

        return Results.Ok(result.Value);
    }

    [HttpPost("forget-browser")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IResult> ForgetBrowser(CancellationToken cancellationToken)
    {
        await _twoFactorService.ForgetBrowserAsync();
        return Results.Ok();
    }
}
