using System.Security.Claims;
using Carter;
using BuildingBlocks.Web.Authorization;
using BuildingBlocks.Application.Constants;
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

    private static async Task<Results<Ok<WalletDto>, NotFound, BadRequest<ProblemDetails>>> GetWallet(
        string username,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetWalletQuery { Username = username }, cancellationToken);

        if (!result.IsSuccess)
            return TypedResults.BadRequest(ProblemDetailsHelper.FromError(result.Error!));

        if (result.Value == null)
            return TypedResults.NotFound();

        return TypedResults.Ok(result.Value);
    }

    private static async Task<Results<Created<WalletDto>, Conflict<ProblemDetails>>> CreateWallet(
        string username,
        ClaimsPrincipal user,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromClaims(user);
        var result = await mediator.Send(new CreateWalletCommand
        {
            UserId = userId,
            Username = username
        }, cancellationToken);

        if (!result.IsSuccess)
            return TypedResults.Conflict(ProblemDetailsHelper.FromError(result.Error!));

        return TypedResults.Created($"/api/v1/wallets/{username}", result.Value);
    }

    private static async Task<Results<Ok<WalletTransactionDto>, NotFound<ProblemDetails>, BadRequest<ProblemDetails>>> Deposit(
        string username,
        DepositDto dto,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DepositCommand
        {
            Username = username,
            Amount = dto.Amount,
            PaymentMethod = dto.PaymentMethod,
            Description = dto.Description
        }, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Error?.Code == "Wallet.NotFound")
                return TypedResults.NotFound(ProblemDetailsHelper.FromError(result.Error));
            return TypedResults.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
        }

        return TypedResults.Ok(result.Value);
    }

    private static async Task<Results<Ok<WalletTransactionDto>, NotFound<ProblemDetails>, BadRequest<ProblemDetails>>> Withdraw(
        string username,
        WithdrawDto dto,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new WithdrawCommand
        {
            Username = username,
            Amount = dto.Amount,
            PaymentMethod = dto.PaymentMethod,
            Description = dto.Description
        }, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Error?.Code == "Wallet.NotFound")
                return TypedResults.NotFound(ProblemDetailsHelper.FromError(result.Error));
            return TypedResults.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
        }

        return TypedResults.Ok(result.Value);
    }

    private static Guid GetUserIdFromClaims(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst("sub")?.Value ?? user.FindFirst("id")?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}
