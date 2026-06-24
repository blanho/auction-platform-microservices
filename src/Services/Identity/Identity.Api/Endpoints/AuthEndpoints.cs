using Carter;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Web;
using BuildingBlocks.Web.Authorization;
using BuildingBlocks.Web.Helpers;
using Identity.Application.DTOs.Auth;
using Identity.Application.DTOs.External;
using Identity.Application.DTOs.TwoFactor;
using Identity.Application.DTOs.Users;
using Identity.Application.Errors;
using Identity.Application.Features.Auth.Commands.Register;
using Identity.Application.Features.Auth.Commands.ConfirmEmail;
using Identity.Application.Features.Auth.Commands.ResendConfirmation;
using Identity.Application.Features.Auth.Commands.ForgotPassword;
using Identity.Application.Features.Auth.Commands.ResetPassword;
using Identity.Application.Features.Auth.Commands.Login;
using Identity.Application.Features.Auth.Commands.LoginWith2FA;
using Identity.Application.Features.Auth.Commands.RefreshToken;
using Identity.Application.Features.Auth.Commands.Logout;
using Identity.Application.Features.Auth.Commands.LogoutAll;
using Identity.Application.Features.Auth.Querys.GetCurrentUser;
using Identity.Application.Features.Auth.Queries.GetExternalLoginInfo;
using Identity.Application.Features.Auth.Commands.ProcessExternalLogin;
using Identity.Application.Features.Auth.Commands.ExchangeCodeForTokens;
using Identity.Application.Features.Auth.Querys.CheckUsernameAvailability;
using Identity.Application.Features.Auth.Querys.CheckEmailAvailability;
using EnvironmentHelper = Identity.Application.Helpers.EnvironmentHelper;
using CookieHelper = Identity.Application.Helpers.CookieHelper;
using HttpContextHelper = Identity.Application.Helpers.HttpContextHelper;

namespace Identity.Api.Endpoints;

public class AuthEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/auth")
            .WithTags("Auth");

        group.MapPost("/register", Register)
            .WithName("Register")
            .RequireRateLimiting("registration")
            .Produces<UserDto>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapPost("/confirm-email", ConfirmEmail)
            .WithName("ConfirmEmail")
            .Produces(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapPost("/resend-confirmation", ResendConfirmation)
            .WithName("ResendConfirmation")
            .RequireRateLimiting("password-reset")
            .Produces(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapPost("/forgot-password", ForgotPassword)
            .WithName("ForgotPassword")
            .RequireRateLimiting("password-reset")
            .Produces(StatusCodes.Status200OK);

        group.MapPost("/reset-password", ResetPassword)
            .WithName("ResetPassword")
            .Produces(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapPost("/login", Login)
            .WithName("Login")
            .RequireRateLimiting("auth")
            .Produces<LoginResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);

        group.MapPost("/login-2fa", LoginWith2FA)
            .WithName("LoginWith2FA")
            .RequireRateLimiting("2fa")
            .Produces<LoginResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);

        group.MapPost("/refresh", RefreshToken)
            .WithName("RefreshToken")
            .Produces<TokenResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status403Forbidden);

        group.MapPost("/logout", Logout)
            .WithName("Logout")
            .RequireAuthorization()
            .Produces(StatusCodes.Status200OK);

        group.MapPost("/logout-all", LogoutAll)
            .WithName("LogoutAll")
            .RequireAuthorization()
            .Produces(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapGet("/me", GetCurrentUser)
            .WithName("GetCurrentUser")
            .RequireAuthorization()
            .Produces<UserDto>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);

        group.MapGet("/external-login/{provider}", ExternalLogin)
            .WithName("ExternalLogin")
            .Produces(StatusCodes.Status302Found)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapGet("/external-login-callback", ExternalLoginCallback)
            .WithName("ExternalLoginCallback");

        group.MapPost("/exchange-code", ExchangeCodeForTokens)
            .WithName("ExchangeCodeForTokens")
            .RequireRateLimiting("auth")
            .Produces<LoginResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapGet("/check-username/{username}", CheckUsername)
            .WithName("CheckUsername")
            .Produces<bool>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapGet("/check-email/{email}", CheckEmail)
            .WithName("CheckEmail")
            .Produces<bool>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> Register([FromBody] RegisterRequest dto, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new RegisterCommand(dto), cancellationToken);
        return result.IsSuccess
            ? Results.Created($"/api/v1/auth/me", result.Value)
            : Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }

    private static async Task<IResult> ConfirmEmail([FromBody] ConfirmEmailRequest request, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ConfirmEmailCommand(request), cancellationToken);
        return result.IsSuccess
            ? Results.Ok()
            : Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }

    private static async Task<IResult> ResendConfirmation([FromBody] ResendConfirmationRequest request, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ResendConfirmationCommand(request.Email), cancellationToken);
        return result.IsSuccess
            ? Results.Ok()
            : Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }

    private static async Task<IResult> ForgotPassword([FromBody] ForgotPasswordRequest request, ISender sender, CancellationToken cancellationToken)
    {
        await sender.Send(new ForgotPasswordCommand(request.Email), cancellationToken);
        return Results.Ok();
    }

    private static async Task<IResult> ResetPassword([FromBody] ResetPasswordRequest request, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ResetPasswordCommand(request), cancellationToken);
        return result.IsSuccess
            ? Results.Ok()
            : Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }

    private static async Task<IResult> Login([FromBody] LoginRequest dto, HttpContext httpContext, IConfiguration configuration, ISender sender, CancellationToken cancellationToken)
    {
        var ipAddress = HttpContextHelper.GetIpAddress(httpContext);
        var result = await sender.Send(new LoginCommand(dto, ipAddress!), cancellationToken);

        if (!result.IsSuccess)
            return Results.Unauthorized();

        if (result.Value!.RequiresTwoFactor)
            return Results.Ok(result.Value);

        if (!string.IsNullOrEmpty(result.Value.RefreshToken))
            CookieHelper.SetRefreshTokenCookie(httpContext.Response, result.Value.RefreshToken, EnvironmentHelper.IsProduction(configuration));

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> LoginWith2FA([FromBody] TwoFactorLoginRequest request, HttpContext httpContext, IConfiguration configuration, ISender sender, CancellationToken cancellationToken)
    {
        var ipAddress = HttpContextHelper.GetIpAddress(httpContext);
        var result = await sender.Send(new LoginWith2FACommand(request, ipAddress!), cancellationToken);

        if (!result.IsSuccess)
            return Results.Unauthorized();

        if (!string.IsNullOrEmpty(result.Value?.RefreshToken))
            CookieHelper.SetRefreshTokenCookie(httpContext.Response, result.Value.RefreshToken, EnvironmentHelper.IsProduction(configuration));

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> RefreshToken(HttpContext httpContext, IConfiguration configuration, ISender sender, CancellationToken cancellationToken)
    {
        var refreshToken = CookieHelper.GetRefreshTokenFromCookie(httpContext.Request);
        if (string.IsNullOrEmpty(refreshToken))
            return Results.Unauthorized();

        var ipAddress = HttpContextHelper.GetIpAddress(httpContext);
        var result = await sender.Send(new RefreshTokenCommand(refreshToken, ipAddress!), cancellationToken);

        if (!result.IsSuccess)
        {
            CookieHelper.ClearRefreshTokenCookie(httpContext.Response, EnvironmentHelper.IsProduction(configuration));

            if (result.Error == IdentityErrors.Auth.SecurityTermination)
                return Results.Json(
                    new { code = "security_termination", message = "Session terminated due to suspicious activity" },
                    statusCode: StatusCodes.Status403Forbidden);

            return Results.Unauthorized();
        }

        if (!string.IsNullOrEmpty(result.Value?.RefreshToken))
            CookieHelper.SetRefreshTokenCookie(httpContext.Response, result.Value.RefreshToken, EnvironmentHelper.IsProduction(configuration));

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> Logout(System.Security.Claims.ClaimsPrincipal user, HttpContext httpContext, IConfiguration configuration, ISender sender, CancellationToken cancellationToken)
    {
        var userId = user.GetRequiredUserIdString();
        var refreshToken = CookieHelper.GetRefreshTokenFromCookie(httpContext.Request) ?? string.Empty;

        await sender.Send(new LogoutCommand(userId, refreshToken), cancellationToken);
        CookieHelper.ClearRefreshTokenCookie(httpContext.Response, EnvironmentHelper.IsProduction(configuration));
        return Results.Ok();
    }

    private static async Task<IResult> LogoutAll(System.Security.Claims.ClaimsPrincipal user, HttpContext httpContext, IConfiguration configuration, ISender sender, CancellationToken cancellationToken)
    {
        var userId = user.GetRequiredUserIdString();
        await sender.Send(new LogoutAllCommand(userId), cancellationToken);
        CookieHelper.ClearRefreshTokenCookie(httpContext.Response, EnvironmentHelper.IsProduction(configuration));
        return Results.Ok();
    }

    private static async Task<IResult> GetCurrentUser(System.Security.Claims.ClaimsPrincipal user, ISender sender, CancellationToken cancellationToken)
    {
        var userId = user.GetRequiredUserIdString();
        var result = await sender.Send(new GetCurrentUserQuery(userId), cancellationToken);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.Unauthorized();
    }

    private static IResult ExternalLogin(string provider, [FromQuery] string? returnUrl, HttpContext httpContext, IConfiguration configuration)
    {
        var allowedProviders = new[] { "Google", "Facebook" };
        if (!allowedProviders.Contains(provider, StringComparer.OrdinalIgnoreCase))
        {
            return Results.BadRequest(ProblemDetailsHelper.ValidationError("provider", $"Provider '{provider}' is not supported"));
        }

        var frontendUrl = configuration["FrontendUrl"] ?? "http://localhost:5173";
        var callbackUrl = $"/api/v1/auth/external-login-callback?returnUrl={HttpUtility.UrlEncode(returnUrl ?? frontendUrl)}";

        var properties = new Microsoft.AspNetCore.Authentication.AuthenticationProperties
        {
            RedirectUri = callbackUrl,
            Items = { { "LoginProvider", provider } }
        };

        return Results.Challenge(properties, new[] { provider });
    }

    private static async Task<IResult> ExternalLoginCallback([FromQuery] string? returnUrl, [FromQuery] string? remoteError, HttpContext httpContext, IConfiguration configuration, ILogger<AuthEndpoints> logger, ISender sender, CancellationToken cancellationToken)
    {
        var frontendUrl = configuration["FrontendUrl"] ?? "http://localhost:3000";
        returnUrl ??= frontendUrl;

        if (remoteError != null)
        {
            logger.LogError("External login error: {Error}", remoteError);
            return Results.Redirect($"{frontendUrl}/auth/signin?error={HttpUtility.UrlEncode(remoteError)}");
        }

        var infoResult = await sender.Send(new GetExternalLoginInfoQuery(), cancellationToken);
        var info = infoResult.IsSuccess ? infoResult.Value : null;
        if (info == null)
        {
            logger.LogError("External login info is null");
            return Results.Redirect($"{frontendUrl}/auth/signin?error=External+login+failed");
        }

        var result = await sender.Send(new ProcessExternalLoginCommand(info), cancellationToken);

        if (!result.IsSuccess)
            return Results.Redirect($"{frontendUrl}/auth/signin?error={HttpUtility.UrlEncode(result.Error!.Message)}");

        return Results.Redirect($"{returnUrl}?code={result.Value!.AuthCode}");
    }

    private static async Task<IResult> ExchangeCodeForTokens([FromBody] ExchangeCodeRequest request, HttpContext httpContext, IConfiguration configuration, ISender sender, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.Code))
            return Results.BadRequest(ProblemDetailsHelper.ValidationError("Code", "Authorization code is required"));

        var ipAddress = HttpContextHelper.GetIpAddress(httpContext);
        var result = await sender.Send(new ExchangeCodeForTokensCommand(request.Code, ipAddress!), cancellationToken);

        if (!result.IsSuccess)
            return Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));

        if (!string.IsNullOrEmpty(result.Value?.RefreshToken))
            CookieHelper.SetRefreshTokenCookie(httpContext.Response, result.Value.RefreshToken, EnvironmentHelper.IsProduction(configuration));

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> CheckUsername(string username, ISender sender, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(username))
            return Results.BadRequest(ProblemDetailsHelper.ValidationError("Username", "Username is required"));

        var availableResult = await sender.Send(new CheckUsernameAvailabilityQuery(username), cancellationToken);
        var available = availableResult.IsSuccess && availableResult.Value;
        return Results.Ok(available);
    }

    private static async Task<IResult> CheckEmail(string email, ISender sender, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Results.BadRequest(ProblemDetailsHelper.ValidationError("Email", "Email is required"));

        var availableResult = await sender.Send(new CheckEmailAvailabilityQuery(email), cancellationToken);
        var available = availableResult.IsSuccess && availableResult.Value;
        return Results.Ok(available);
    }
}
