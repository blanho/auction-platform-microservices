using System.Security.Claims;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using BuildingBlocks.Web.Authorization;
using BuildingBlocks.Web.Helpers;
using Identity.Application.DTOs.Seller;
using Identity.Application.DTOs.TwoFactor;
using Identity.Application.DTOs.Users;
using Identity.Application.Errors;
using Identity.Application.Features.Users.Queries.GetUsers;
using Identity.Application.Features.Users.Queries.GetUserById;
using Identity.Application.Features.Users.Queries.GetSellerStatus;
using Identity.Application.Features.Users.Commands.ApplyForSeller;
using Identity.Application.Features.Users.Commands.SuspendUser;
using Identity.Application.Features.Users.Commands.UnsuspendUser;
using Identity.Application.Features.Users.Commands.ActivateUser;
using Identity.Application.Features.Users.Commands.DeactivateUser;
using Identity.Application.Features.Users.Commands.UpdateUserRoles;
using Identity.Application.Features.Users.Commands.DeleteUser;
using Identity.Application.Features.Users.Queries.GetAdminStats;
using Identity.Application.Features.TwoFactor.Queries.GetStatusByAdmin;
using Identity.Application.Features.TwoFactor.Commands.ResetByAdmin;
using Identity.Application.Features.TwoFactor.Commands.DisableByAdmin;

namespace Identity.Api.Endpoints.Users;

public class UserEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/users")
            .WithTags("Users")
            .RequireAuthorization();

        group.MapGet("", GetUsers)
            .WithName("GetUsers")
            .Produces<PaginatedResult<AdminUserDto>>(StatusCodes.Status200OK);

        group.MapGet("/{id}", GetUser)
            .WithName("GetUser")
            .Produces<AdminUserDto>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapGet("/seller/status", GetSellerStatus)
            .WithName("GetSellerStatus")
            .Produces<SellerStatusResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);

        group.MapPost("/seller/apply", ApplyForSeller)
            .WithName("ApplyForSeller")
            .Produces<SellerStatusResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);

        group.MapPost("/{id}/suspend", SuspendUser)
            .WithName("SuspendUser")
            .Produces(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapPost("/{id}/unsuspend", UnsuspendUser)
            .WithName("UnsuspendUser")
            .Produces(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapPost("/{id}/activate", ActivateUser)
            .WithName("ActivateUser")
            .Produces(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapPost("/{id}/deactivate", DeactivateUser)
            .WithName("DeactivateUser")
            .Produces(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapPut("/{id}/roles", UpdateUserRoles)
            .WithName("UpdateUserRoles")
            .Produces(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapDelete("/{id}", DeleteUser)
            .WithName("DeleteUser")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapGet("/stats", GetStats)
            .WithName("GetAdminStats")
            .Produces<AdminStatsResponse>(StatusCodes.Status200OK);

        group.MapGet("/{id}/2fa/status", GetUser2FAStatus)
            .WithName("GetUser2FAStatus")
            .Produces<TwoFactorStatusResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapPost("/{id}/2fa/reset", Reset2FA)
            .WithName("Reset2FA")
            .Produces(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapPost("/{id}/2fa/disable", Disable2FA)
            .WithName("Disable2FA")
            .Produces(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> GetUsers([AsParameters] GetUsersQuery query, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetUsersListQuery(query), cancellationToken);
        return Results.Ok(result.Value);
    }

    private static async Task<IResult> GetUser(string id, ISender sender, CancellationToken cancellationToken)
    {
        var user = await sender.Send(new GetUserByIdQuery(id), cancellationToken);
        return user == null
            ? Results.NotFound(ProblemDetailsHelper.NotFound("User", id))
            : Results.Ok(user);
    }

    private static async Task<IResult> GetSellerStatus(ClaimsPrincipal user, ISender sender, CancellationToken cancellationToken)
    {
        var userId = user.GetRequiredUserIdString();
        var result = await sender.Send(new GetSellerStatusQuery(userId), cancellationToken);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.Unauthorized();
    }

    private static async Task<IResult> ApplyForSeller([FromBody] BecomeSellerRequest request, ClaimsPrincipal user, ISender sender, CancellationToken cancellationToken)
    {
        var userId = user.GetRequiredUserIdString();
        var result = await sender.Send(new ApplyForSellerCommand(userId, request.AcceptTerms), cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Error == IdentityErrors.User.NotFound)
                return Results.Unauthorized();
            return Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
        }

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> SuspendUser(string id, [FromBody] SuspendUserRequest request, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SuspendUserCommand(id, request.Reason), cancellationToken);
        return result.IsSuccess
            ? Results.Ok()
            : Results.NotFound(ProblemDetailsHelper.NotFound("User", id));
    }

    private static async Task<IResult> UnsuspendUser(string id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UnsuspendUserCommand(id), cancellationToken);
        return result.IsSuccess
            ? Results.Ok()
            : Results.NotFound(ProblemDetailsHelper.NotFound("User", id));
    }

    private static async Task<IResult> ActivateUser(string id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ActivateUserCommand(id), cancellationToken);
        return result.IsSuccess
            ? Results.Ok()
            : Results.NotFound(ProblemDetailsHelper.NotFound("User", id));
    }

    private static async Task<IResult> DeactivateUser(string id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeactivateUserCommand(id), cancellationToken);
        return result.IsSuccess
            ? Results.Ok()
            : Results.NotFound(ProblemDetailsHelper.NotFound("User", id));
    }

    private static async Task<IResult> UpdateUserRoles(string id, [FromBody] UpdateUserRolesRequest request, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateUserRolesCommand(id, request.Roles), cancellationToken);
        return result.IsSuccess
            ? Results.Ok()
            : Results.NotFound(ProblemDetailsHelper.NotFound("User", id));
    }

    private static async Task<IResult> DeleteUser(string id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteUserCommand(id), cancellationToken);
        return result.IsSuccess
            ? Results.NoContent()
            : Results.NotFound(ProblemDetailsHelper.NotFound("User", id));
    }

    private static async Task<IResult> GetStats(ISender sender, CancellationToken cancellationToken)
    {
        var statsResult = await sender.Send(new GetAdminStatsQuery(), cancellationToken);
        return Results.Ok(statsResult.Value);
    }

    private static async Task<IResult> GetUser2FAStatus(string id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetStatusByAdminQuery(id), cancellationToken);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.NotFound(ProblemDetailsHelper.NotFound("User", id));
    }

    private static async Task<IResult> Reset2FA(string id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ResetByAdminCommand(id), cancellationToken);
        return result.IsSuccess
            ? Results.Ok()
            : Results.NotFound(ProblemDetailsHelper.NotFound("User", id));
    }

    private static async Task<IResult> Disable2FA(string id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DisableByAdminCommand(id), cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Error == IdentityErrors.User.NotFound)
                return Results.NotFound(ProblemDetailsHelper.NotFound("User", id));
            return Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
        }

        return Results.Ok();
    }
}
