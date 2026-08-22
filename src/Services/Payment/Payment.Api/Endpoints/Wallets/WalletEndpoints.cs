using System.Security.Claims;
using Carter;
using BuildingBlocks.Web.Authorization;
using BuildingBlocks.Application.Constants;
using BuildingBlocks.Web.Extensions;
using BuildingBlocks.Web.Helpers;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.Features.Wallets.CreateWallet;
using Payment.Application.DTOs;
using Payment.Application.Features.Wallets.GetWallet;

namespace Payment.Api.Endpoints.Wallets;

public class WalletEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/wallets")
            .WithTags("Wallets")
            .RequireAuthorization();

        group.MapGet("/{username}", GetWallet)
            .WithName("GetWallet")
            .WithSummary("Get wallet by username")
            .RequireAuthorization();

        group.MapPost("/{username}/create", CreateWallet)
            .WithName("CreateWallet")
            .WithSummary("Create a wallet for the authenticated user");
    }

    private static async Task<IResult> GetWallet(
        string username,
        ClaimsPrincipal user,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var authenticatedUsername = UserHelper.GetUsername(user);
        if (!string.Equals(authenticatedUsername, username, StringComparison.OrdinalIgnoreCase) &&
            !user.HasPermission(Permissions.Wallets.View))
            return Results.Forbid();

        var result = await mediator.Send(new GetWalletQuery { Username = username }, cancellationToken);

        return result.ToOkResult();
    }

    private static async Task<IResult> CreateWallet(
        string username,
        ClaimsPrincipal user,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var authenticatedUsername = UserHelper.GetUsername(user);
        if (!string.Equals(authenticatedUsername, username, StringComparison.OrdinalIgnoreCase))
            return Results.Forbid();

        var userId = UserHelper.GetUserId(user) ?? Guid.Empty;
        var result = await mediator.Send(new CreateWalletCommand
        {
            UserId = userId,
            Username = username
        }, cancellationToken);

        return result.ToApiResult(wallet =>
            Results.Created($"/api/v1/wallets/{username}", wallet));
    }
}
