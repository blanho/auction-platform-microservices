using BuildingBlocks.Web.Authorization;
using BuildingBlocks.Web.Helpers;
using Identity.Application.DTOs.TwoFactor;
using Identity.Application.Errors;
using Identity.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Api.Controllers;

[ApiController]
[Route("api/auth/2fa")]
[Authorize]
[Produces("application/json")]
public class TwoFactorController : ControllerBase
{
    private readonly MediatR.ISender _sender;

    public TwoFactorController(MediatR.ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("status")]
    [ProducesResponseType(typeof(TwoFactorStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IResult> GetStatus(CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserIdString();
        var result = await _sender.Send(new Identity.Application.Features.TwoFactor.Queries.GetStatus.GetStatusQuery(userId), cancellationToken);
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
        var userId = User.GetRequiredUserIdString();
        var result = await _sender.Send(new Identity.Application.Features.TwoFactor.Commands.SetupAuthenticator.SetupAuthenticatorCommand(userId), cancellationToken);

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
        var userId = User.GetRequiredUserIdString();
        var result = await _sender.Send(new Identity.Application.Features.TwoFactor.Commands.EnableAuthenticator.EnableAuthenticatorCommand(userId, request.Code), cancellationToken);

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
        var userId = User.GetRequiredUserIdString();
        var result = await _sender.Send(new Identity.Application.Features.TwoFactor.Commands.DisableAuthenticator.DisableAuthenticatorCommand(userId, request.Password), cancellationToken);

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
        var result = await _sender.Send(new Identity.Application.Features.TwoFactor.Commands.VerifyCode.VerifyCodeCommand(request.Code, request.RememberDevice), cancellationToken);
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
        var result = await _sender.Send(new Identity.Application.Features.TwoFactor.Commands.UseRecoveryCode.UseRecoveryCodeCommand(request.RecoveryCode), cancellationToken);
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
        var userId = User.GetRequiredUserIdString();
        var result = await _sender.Send(new Identity.Application.Features.TwoFactor.Commands.GenerateRecoveryCodes.GenerateRecoveryCodesCommand(userId), cancellationToken);

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
        await _sender.Send(new Identity.Application.Features.TwoFactor.Commands.ForgetBrowser.ForgetBrowserCommand(), cancellationToken);
        return Results.Ok();
    }
}
