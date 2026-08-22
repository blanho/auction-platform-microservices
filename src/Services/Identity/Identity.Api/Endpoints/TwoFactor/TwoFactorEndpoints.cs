using System.Security.Claims;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using BuildingBlocks.Web.Authorization;
using BuildingBlocks.Web.Helpers;
using Identity.Application.DTOs.TwoFactor;
using Identity.Application.Errors;
using Identity.Application.Features.TwoFactor.Queries.GetStatus;
using Identity.Application.Features.TwoFactor.Commands.SetupAuthenticator;
using Identity.Application.Features.TwoFactor.Commands.EnableAuthenticator;
using Identity.Application.Features.TwoFactor.Commands.DisableAuthenticator;
using Identity.Application.Features.TwoFactor.Commands.GenerateRecoveryCodes;
using Identity.Application.Features.TwoFactor.Commands.ForgetBrowser;

namespace Identity.Api.Endpoints.TwoFactor;

public class TwoFactorEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/auth/2fa")
            .WithTags("TwoFactor")
            .RequireAuthorization();

        group.MapGet("/status", GetStatus)
            .WithName("Get2FAStatus")
            .Produces<TwoFactorStatusResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapPost("/setup", SetupAuthenticator)
            .WithName("SetupAuthenticator")
            .RequireRateLimiting("2fa")
            .Produces<TwoFactorSetupResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapPost("/enable", EnableAuthenticator)
            .WithName("EnableAuthenticator")
            .RequireRateLimiting("2fa")
            .Produces<RecoveryCodesResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapPost("/disable", DisableAuthenticator)
            .WithName("DisableAuthenticator")
            .RequireRateLimiting("2fa")
            .Produces(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapPost("/verify", VerifyAndEnableAuthenticator)
            .WithName("Verify2FACode")
            .RequireRateLimiting("2fa")
            .Produces<RecoveryCodesResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapPost("/generate-codes", GenerateRecoveryCodes)
            .WithName("GenerateRecoveryCodes")
            .RequireRateLimiting("2fa")
            .Produces<RecoveryCodesResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapPost("/forget-browser", ForgetBrowser)
            .WithName("ForgetBrowser")
            .Produces(StatusCodes.Status200OK);
    }

    private static async Task<IResult> GetStatus(ClaimsPrincipal user, ISender sender, CancellationToken cancellationToken)
    {
        var userId = user.GetRequiredUserIdString();
        var result = await sender.Send(new GetStatusQuery(userId), cancellationToken);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.NotFound(ProblemDetailsHelper.NotFound("User", userId));
    }

    private static async Task<IResult> SetupAuthenticator(ClaimsPrincipal user, ISender sender, CancellationToken cancellationToken)
    {
        var userId = user.GetRequiredUserIdString();
        var result = await sender.Send(new SetupAuthenticatorCommand(userId), cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Error == IdentityErrors.User.NotFound)
                return Results.NotFound(ProblemDetailsHelper.NotFound("User", userId));
            return Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
        }

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> EnableAuthenticator([FromBody] Enable2FARequest request, ClaimsPrincipal user, ISender sender, CancellationToken cancellationToken)
    {
        var userId = user.GetRequiredUserIdString();
        var result = await sender.Send(new EnableAuthenticatorCommand(userId, request.Code), cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Error == IdentityErrors.User.NotFound)
                return Results.NotFound(ProblemDetailsHelper.NotFound("User", userId));
            return Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
        }

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> DisableAuthenticator([FromBody] Disable2FARequest request, ClaimsPrincipal user, ISender sender, CancellationToken cancellationToken)
    {
        var userId = user.GetRequiredUserIdString();
        var result = await sender.Send(new DisableAuthenticatorCommand(userId, request.Password), cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Error == IdentityErrors.User.NotFound)
                return Results.NotFound(ProblemDetailsHelper.NotFound("User", userId));
            return Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
        }

        return Results.Ok();
    }

    private static async Task<IResult> VerifyAndEnableAuthenticator(
        [FromBody] Verify2FARequest request,
        ClaimsPrincipal user,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var userId = user.GetRequiredUserIdString();
        var result = await sender.Send(
            new EnableAuthenticatorCommand(userId, request.Code),
            cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Error == IdentityErrors.User.NotFound)
                return Results.NotFound(ProblemDetailsHelper.NotFound("User", userId));
            return Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
        }

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> GenerateRecoveryCodes(ClaimsPrincipal user, ISender sender, CancellationToken cancellationToken)
    {
        var userId = user.GetRequiredUserIdString();
        var result = await sender.Send(new GenerateRecoveryCodesCommand(userId), cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Error == IdentityErrors.User.NotFound)
                return Results.NotFound(ProblemDetailsHelper.NotFound("User", userId));
            return Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
        }

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> ForgetBrowser(ISender sender, CancellationToken cancellationToken)
    {
        await sender.Send(new ForgetBrowserCommand(), cancellationToken);
        return Results.Ok();
    }
}
