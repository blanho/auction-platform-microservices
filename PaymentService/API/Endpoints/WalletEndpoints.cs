using System.Security.Claims;
using Carter;
using Common.Utilities.Helpers;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PaymentService.Application.Commands.CreateWallet;
using PaymentService.Application.Commands.Deposit;
using PaymentService.Application.Commands.Withdraw;
using PaymentService.Application.DTOs;
using PaymentService.Application.Queries.GetTransactionById;
using PaymentService.Application.Queries.GetWallet;
using PaymentService.Application.Queries.GetWalletTransactions;

namespace PaymentService.API.Endpoints;

public class WalletEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/wallets")
            .WithTags("Wallets")
            .RequireAuthorization();

        group.MapGet("/{username}", GetWallet)
            .WithName("GetWallet")
            .WithSummary("Get wallet by username");

        group.MapPost("/{username}/create", CreateWallet)
            .WithName("CreateWallet")
            .WithSummary("Create a wallet for a user");

        group.MapPost("/{username}/deposit", Deposit)
            .WithName("DepositToWallet")
            .WithSummary("Deposit funds to wallet");

        group.MapPost("/{username}/withdraw", Withdraw)
            .WithName("WithdrawFromWallet")
            .WithSummary("Withdraw funds from wallet");

        group.MapGet("/{username}/transactions", GetTransactions)
            .WithName("GetWalletTransactions")
            .WithSummary("Get wallet transactions");

        group.MapGet("/transactions/{id:guid}", GetTransaction)
            .WithName("GetWalletTransaction")
            .WithSummary("Get a specific transaction by ID");
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

    private static async Task<Results<Ok<IEnumerable<WalletTransactionDto>>, BadRequest<ProblemDetails>>> GetTransactions(
        string username,
        int page,
        int pageSize,
        IMediator mediator,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetWalletTransactionsQuery
        {
            Username = username,
            Page = page > 0 ? page : 1,
            PageSize = pageSize > 0 ? pageSize : 10
        }, cancellationToken);

        if (!result.IsSuccess)
            return TypedResults.BadRequest(ProblemDetailsHelper.FromError(result.Error!));

        httpContext.Response.Headers.Append("X-Total-Count", result.Value.TotalCount.ToString());
        httpContext.Response.Headers.Append("X-Page", (page > 0 ? page : 1).ToString());
        httpContext.Response.Headers.Append("X-Page-Size", (pageSize > 0 ? pageSize : 10).ToString());

        return TypedResults.Ok(result.Value.Transactions);
    }

    private static async Task<Results<Ok<WalletTransactionDto>, NotFound, BadRequest<ProblemDetails>>> GetTransaction(
        Guid id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetTransactionByIdQuery { TransactionId = id }, cancellationToken);

        if (!result.IsSuccess)
            return TypedResults.BadRequest(ProblemDetailsHelper.FromError(result.Error!));

        if (result.Value == null)
            return TypedResults.NotFound();

        return TypedResults.Ok(result.Value);
    }

    private static Guid GetUserIdFromClaims(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst("sub")?.Value ?? user.FindFirst("id")?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}
