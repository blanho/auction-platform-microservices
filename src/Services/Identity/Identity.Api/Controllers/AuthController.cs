using BuildingBlocks.Web.Authorization;
using BuildingBlocks.Web.Helpers;
using Identity.Application.DTOs.Auth;
using Identity.Application.DTOs.External;
using Identity.Application.DTOs.TwoFactor;
using Identity.Application.DTOs.Users;
using Identity.Application.Errors;
using Identity.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Web;
using EnvironmentHelper = Identity.Application.Helpers.EnvironmentHelper;
using CookieHelper = Identity.Application.Helpers.CookieHelper;
using HttpContextHelper = Identity.Application.Helpers.HttpContextHelper;

namespace Identity.Api.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly MediatR.ISender _sender;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        MediatR.ISender sender,
        IConfiguration configuration,
        ILogger<AuthController> logger)
    {
        _sender = sender;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("register")]
    [EnableRateLimiting("registration")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IResult> Register([FromBody] RegisterRequest dto, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new Identity.Application.Features.Auth.Commands.Register.RegisterCommand(dto), cancellationToken);
        return result.IsSuccess
            ? Results.Created($"/api/auth/me", result.Value)
            : Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }

    [HttpPost("confirm-email")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IResult> ConfirmEmail([FromBody] ConfirmEmailRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new Identity.Application.Features.Auth.Commands.ConfirmEmail.ConfirmEmailCommand(request), cancellationToken);
        return result.IsSuccess
            ? Results.Ok()
            : Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }

    [HttpPost("resend-confirmation")]
    [EnableRateLimiting("password-reset")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IResult> ResendConfirmation([FromBody] ResendConfirmationRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new Identity.Application.Features.Auth.Commands.ResendConfirmation.ResendConfirmationCommand(request.Email), cancellationToken);
        return result.IsSuccess
            ? Results.Ok()
            : Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }

    [HttpPost("forgot-password")]
    [EnableRateLimiting("password-reset")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        await _sender.Send(new Identity.Application.Features.Auth.Commands.ForgotPassword.ForgotPasswordCommand(request.Email), cancellationToken);
        return Results.Ok();
    }

    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new Identity.Application.Features.Auth.Commands.ResetPassword.ResetPasswordCommand(request), cancellationToken);
        return result.IsSuccess
            ? Results.Ok()
            : Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }

    [HttpPost("login")]
    [EnableRateLimiting("auth")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IResult> Login([FromBody] LoginRequest dto, CancellationToken cancellationToken)
    {
        var ipAddress = HttpContextHelper.GetIpAddress(HttpContext);
        var result = await _sender.Send(new Identity.Application.Features.Auth.Commands.Login.LoginCommand(dto, ipAddress!), cancellationToken);

        if (!result.IsSuccess)
            return Results.Unauthorized();

        if (result.Value!.RequiresTwoFactor)
            return Results.Ok(result.Value);

        if (!string.IsNullOrEmpty(result.Value.RefreshToken))
            CookieHelper.SetRefreshTokenCookie(Response, result.Value.RefreshToken, EnvironmentHelper.IsProduction(_configuration));

        return Results.Ok(result.Value);
    }

    [HttpPost("login-2fa")]
    [EnableRateLimiting("2fa")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IResult> LoginWith2FA([FromBody] TwoFactorLoginRequest request, CancellationToken cancellationToken)
    {
        var ipAddress = HttpContextHelper.GetIpAddress(HttpContext);
        var result = await _sender.Send(new Identity.Application.Features.Auth.Commands.LoginWith2FA.LoginWith2FACommand(request, ipAddress!), cancellationToken);

        if (!result.IsSuccess)
            return Results.Unauthorized();

        if (!string.IsNullOrEmpty(result.Value?.RefreshToken))
            CookieHelper.SetRefreshTokenCookie(Response, result.Value.RefreshToken, EnvironmentHelper.IsProduction(_configuration));

        return Results.Ok(result.Value);
    }

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IResult> RefreshToken(CancellationToken cancellationToken)
    {
        var refreshToken = CookieHelper.GetRefreshTokenFromCookie(Request);
        if (string.IsNullOrEmpty(refreshToken))
            return Results.Unauthorized();

        var ipAddress = HttpContextHelper.GetIpAddress(HttpContext);
        var result = await _sender.Send(new Identity.Application.Features.Auth.Commands.RefreshToken.RefreshTokenCommand(refreshToken, ipAddress!), cancellationToken);

        if (!result.IsSuccess)
        {
            CookieHelper.ClearRefreshTokenCookie(Response, EnvironmentHelper.IsProduction(_configuration));

            if (result.Error == IdentityErrors.Auth.SecurityTermination)
                return Results.Json(
                    new { code = "security_termination", message = "Session terminated due to suspicious activity" },
                    statusCode: StatusCodes.Status403Forbidden);

            return Results.Unauthorized();
        }

        if (!string.IsNullOrEmpty(result.Value?.RefreshToken))
            CookieHelper.SetRefreshTokenCookie(Response, result.Value.RefreshToken, EnvironmentHelper.IsProduction(_configuration));

        return Results.Ok(result.Value);
    }

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IResult> Logout(CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserIdString();
        var refreshToken = CookieHelper.GetRefreshTokenFromCookie(Request) ?? string.Empty;

        await _sender.Send(new Identity.Application.Features.Auth.Commands.Logout.LogoutCommand(userId, refreshToken), cancellationToken);
        CookieHelper.ClearRefreshTokenCookie(Response, EnvironmentHelper.IsProduction(_configuration));
        return Results.Ok();
    }

    [HttpPost("logout-all")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IResult> LogoutAll(CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserIdString();
        await _sender.Send(new Identity.Application.Features.Auth.Commands.LogoutAll.LogoutAllCommand(userId), cancellationToken);
        CookieHelper.ClearRefreshTokenCookie(Response, EnvironmentHelper.IsProduction(_configuration));
        return Results.Ok();
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserIdString();
        var result = await _sender.Send(new Identity.Application.Features.Auth.Querys.GetCurrentUser.GetCurrentUserQuery(userId), cancellationToken);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.Unauthorized();
    }

    [HttpGet("external-login/{provider}")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult ExternalLogin(string provider, [FromQuery] string? returnUrl = null)
    {
        var allowedProviders = new[] { "Google", "Facebook" };
        if (!allowedProviders.Contains(provider, StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest(ProblemDetailsHelper.ValidationError("provider", $"Provider '{provider}' is not supported"));
        }

        var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:5173";
        var callbackUrl = Url.Action(
            nameof(ExternalLoginCallback),
            "Auth",
            new { returnUrl = returnUrl ?? frontendUrl },
            Request.Scheme);

        var properties = new Microsoft.AspNetCore.Authentication.AuthenticationProperties
        {
            RedirectUri = callbackUrl,
            Items = { { "LoginProvider", provider } }
        };

        return Challenge(properties, provider);
    }

    [HttpGet("external-login-callback")]
    public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null, CancellationToken cancellationToken = default)
    {
        var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:3000";
        returnUrl ??= frontendUrl;

        if (remoteError != null)
        {
            _logger.LogError("External login error: {Error}", remoteError);
            return Redirect($"{frontendUrl}/auth/signin?error={HttpUtility.UrlEncode(remoteError)}");
        }

        var infoResult = await _sender.Send(new Identity.Application.Features.Auth.Queries.GetExternalLoginInfo.GetExternalLoginInfoQuery(), cancellationToken);
        var info = infoResult.IsSuccess ? infoResult.Value : null;
        if (info == null)
        {
            _logger.LogError("External login info is null");
            return Redirect($"{frontendUrl}/auth/signin?error=External+login+failed");
        }

        var result = await _sender.Send(new Identity.Application.Features.Auth.Commands.ProcessExternalLogin.ProcessExternalLoginCommand(info), cancellationToken);

        if (!result.IsSuccess)
            return Redirect($"{frontendUrl}/auth/signin?error={HttpUtility.UrlEncode(result.Error!.Message)}");

        return Redirect($"{returnUrl}?code={result.Value!.AuthCode}");
    }

    [HttpPost("exchange-code")]
    [EnableRateLimiting("auth")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IResult> ExchangeCodeForTokens([FromBody] ExchangeCodeRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.Code))
            return Results.BadRequest(ProblemDetailsHelper.ValidationError("Code", "Authorization code is required"));

        var ipAddress = HttpContextHelper.GetIpAddress(HttpContext);
        var result = await _sender.Send(new Identity.Application.Features.Auth.Commands.ExchangeCodeForTokens.ExchangeCodeForTokensCommand(request.Code, ipAddress!), cancellationToken);

        if (!result.IsSuccess)
            return Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));

        if (!string.IsNullOrEmpty(result.Value?.RefreshToken))
            CookieHelper.SetRefreshTokenCookie(Response, result.Value.RefreshToken, EnvironmentHelper.IsProduction(_configuration));

        return Results.Ok(result.Value);
    }

    [HttpGet("check-username/{username}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IResult> CheckUsername(string username, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(username))
            return Results.BadRequest(ProblemDetailsHelper.ValidationError("Username", "Username is required"));

        var availableResult = await _sender.Send(new Identity.Application.Features.Auth.Querys.CheckUsernameAvailability.CheckUsernameAvailabilityQuery(username), cancellationToken);
        var available = availableResult.IsSuccess && availableResult.Value;
        return Results.Ok(available);
    }

    [HttpGet("check-email/{email}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IResult> CheckEmail(string email, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Results.BadRequest(ProblemDetailsHelper.ValidationError("Email", "Email is required"));

        var availableResult = await _sender.Send(new Identity.Application.Features.Auth.Querys.CheckEmailAvailability.CheckEmailAvailabilityQuery(email), cancellationToken);
        var available = availableResult.IsSuccess && availableResult.Value;
        return Results.Ok(available);
    }
}
