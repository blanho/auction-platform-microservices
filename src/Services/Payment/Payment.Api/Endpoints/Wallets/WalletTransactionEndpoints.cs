using Carter;
using BuildingBlocks.Web.Authorization;
using BuildingBlocks.Application.Constants;
using BuildingBlocks.Web.Extensions;
using BuildingBlocks.Web.Helpers;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.DTOs;
using Payment.Application.Features.Wallets.GetTransactionById;
using Payment.Application.Features.Wallets.GetWalletTransactions;

namespace Payment.Api.Endpoints.Wallets;

public class WalletTransactionEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/wallets")
            .WithTags("Wallet Transactions")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Wallets.View));

        group.MapGet("/{username}/transactions", GetTransactions)
            .WithName("GetWalletTransactions")
            .WithSummary("Get wallet transactions");

        group.MapGet("/transactions/{id:guid}", GetTransaction)
            .WithName("GetWalletTransaction")
            .WithSummary("Get a specific transaction by ID");
    }

    private static async Task<IResult> GetTransactions(
        string username,
        int page,
        int pageSize,
        IMediator mediator,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var effectivePage = page > 0 ? page : PaginationDefaults.DefaultPage;
        var effectivePageSize = pageSize > 0 ? pageSize : PaginationDefaults.DefaultPageSize;

        var result = await mediator.Send(new GetWalletTransactionsQuery
        {
            Username = username,
            Page = effectivePage,
            PageSize = effectivePageSize
        }, cancellationToken);

        return result.ToApiResult(paginatedResult =>
        {
            httpContext.Response.Headers.Append("X-Total-Count", paginatedResult.TotalCount.ToString());
            httpContext.Response.Headers.Append("X-Page", effectivePage.ToString());
            httpContext.Response.Headers.Append("X-Page-Size", effectivePageSize.ToString());

            return Results.Ok(paginatedResult.Items);
        });
    }

    private static async Task<IResult> GetTransaction(
        Guid id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetTransactionByIdQuery { TransactionId = id }, cancellationToken);

        return result.ToOkResult();
    }
}
