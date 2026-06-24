using Carter;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using BuildingBlocks.Web.Authorization;
using BuildingBlocks.Web.Helpers;
using Identity.Application.DTOs.Auth;
using Identity.Application.DTOs.Profile;
using Identity.Application.Errors;
using Identity.Application.Features.Profile.Queries.GetProfile;
using Identity.Application.Features.Profile.Commands.UpdateProfile;
using Identity.Application.Features.Profile.Commands.ChangePassword;
using Identity.Application.Features.Profile.Commands.EnableTwoFactor;
using Identity.Application.Features.Profile.Commands.DisableTwoFactor;

namespace Identity.Api.Endpoints;

public class ProfileEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/profile")
            .WithTags("Profile")
            .RequireAuthorization();

        group.MapGet("", GetProfile)
            .WithName("GetProfile")
            .Produces<UserProfileDto>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapPut("", UpdateProfile)
            .WithName("UpdateProfile")
            .Produces<UserProfileDto>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapPost("/change-password", ChangePassword)
            .WithName("ChangePassword")
            .Produces(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapPost("/enable-2fa", EnableTwoFactor)
            .WithName("EnableTwoFactor")
            .Produces(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapPost("/disable-2fa", DisableTwoFactor)
            .WithName("DisableTwoFactor")
            .Produces(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GetProfile(
        System.Security.Claims.ClaimsPrincipal user,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var userId = user.GetRequiredUserIdString();
        var result = await sender.Send(new GetProfileQuery(userId), cancellationToken);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.NotFound(ProblemDetailsHelper.NotFound("Profile", userId));
    }

    private static async Task<IResult> UpdateProfile(
        [FromBody] UpdateProfileRequest request,
        System.Security.Claims.ClaimsPrincipal user,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var userId = user.GetRequiredUserIdString();
        var result = await sender.Send(new UpdateProfileCommand(userId, request), cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Error == IdentityErrors.User.NotFound)
                return Results.NotFound(ProblemDetailsHelper.NotFound("Profile", userId));
            return Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
        }

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        System.Security.Claims.ClaimsPrincipal user,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var userId = user.GetRequiredUserIdString();
        var result = await sender.Send(new ChangePasswordCommand(userId, request), cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Error == IdentityErrors.User.NotFound)
                return Results.NotFound(ProblemDetailsHelper.NotFound("User", userId));
            return Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
        }

        return Results.Ok();
    }

    private static async Task<IResult> EnableTwoFactor(
        System.Security.Claims.ClaimsPrincipal user,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var userId = user.GetRequiredUserIdString();
        var result = await sender.Send(new EnableTwoFactorCommand(userId), cancellationToken);
        return result.IsSuccess
            ? Results.Ok()
            : Results.NotFound(ProblemDetailsHelper.NotFound("User", userId));
    }

    private static async Task<IResult> DisableTwoFactor(
        System.Security.Claims.ClaimsPrincipal user,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var userId = user.GetRequiredUserIdString();
        var result = await sender.Send(new DisableTwoFactorCommand(userId), cancellationToken);
        return result.IsSuccess
            ? Results.Ok()
            : Results.NotFound(ProblemDetailsHelper.NotFound("User", userId));
    }
}
