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
using Payment.Application.Features.Wallets.Deposit;
using Payment.Application.Features.Wallets.Withdraw;
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
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Wallets.View));

        group.MapPost("/{username}/create", CreateWallet)
            .WithName("CreateWallet")
            .WithSummary("Create a wallet for a user")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Wallets.Deposit));

        group.MapPost("/{username}/deposit", Deposit)
            .WithName("DepositToWallet")
            .WithSummary("Deposit funds to wallet")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Wallets.Deposit));

        group.MapPost("/{username}/withdraw", Withdraw)
            .WithName("WithdrawFromWallet")
            .WithSummary("Withdraw funds from wallet")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Wallets.Withdraw));
    }

    private static async Task<IResult> GetWallet(
        string username,
        ClaimsPrincipal user,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var authenticatedUsername = UserHelper.GetUsername(user);
        if (!string.Equals(authenticatedUsername, username, StringComparison.OrdinalIgnoreCase))
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
        var userId = UserHelper.GetUserId(user) ?? Guid.Empty;
        var result = await mediator.Send(new CreateWalletCommand
        {
            UserId = userId,
            Username = username
        }, cancellationToken);

        return result.ToApiResult(wallet =>
            Results.Created($"/api/v1/wallets/{username}", wallet));
    }

    private static async Task<IResult> Deposit(
        string username,
        DepositDto dto,
        ClaimsPrincipal user,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var authenticatedUsername = UserHelper.GetUsername(user);
        if (!string.Equals(authenticatedUsername, username, StringComparison.OrdinalIgnoreCase))
            return Results.Forbid();

        var result = await mediator.Send(new DepositCommand
        {
            Username = username,
            Amount = dto.Amount,
            PaymentMethod = dto.PaymentMethod,
            Description = dto.Description
        }, cancellationToken);

        return result.ToOkResult();
    }

    private static async Task<IResult> Withdraw(
        string username,
        WithdrawDto dto,
        ClaimsPrincipal user,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var authenticatedUsername = UserHelper.GetUsername(user);
        if (!string.Equals(authenticatedUsername, username, StringComparison.OrdinalIgnoreCase))
            return Results.Forbid();

        var result = await mediator.Send(new WithdrawCommand
        {
            Username = username,
            Amount = dto.Amount,
            PaymentMethod = dto.PaymentMethod,
            Description = dto.Description
        }, cancellationToken);

        return result.ToOkResult();
    }
}
