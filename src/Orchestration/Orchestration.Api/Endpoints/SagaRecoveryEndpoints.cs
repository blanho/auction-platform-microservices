using System.Security.Claims;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Orchestration.Infrastructure.Persistence;
using OrchestrationService.Contracts.Events;

namespace Orchestration.Api.Endpoints;

public static class SagaRecoveryEndpoints
{
    public static void MapSagaRecovery(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/orchestration/recovery")
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });
        group.MapGet("/", GetUnresolvedCasesAsync);
        group.MapPost("/{workflow}/{correlationId:guid}/{step}/retry", RetryAsync);
    }

    private static async Task<IResult> GetUnresolvedCasesAsync(
        OrchestrationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var recoveryCases = await dbContext.RecoveryCases
            .AsNoTracking()
            .Where(recoveryCase => recoveryCase.ResolvedAt == null)
            .OrderBy(recoveryCase => recoveryCase.CreatedAt)
            .Take(100)
            .ToListAsync(cancellationToken);

        return Results.Ok(recoveryCases);
    }

    private static async Task<IResult> RetryAsync(
        string workflow,
        Guid correlationId,
        string step,
        OrchestrationDbContext dbContext,
        IPublishEndpoint publisher,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        await dbContext.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock(hashtextextended({workflow + correlationId}, 0))", cancellationToken);
        var recoveryCase = await dbContext.RecoveryCases.FindAsync([workflow, correlationId, step], cancellationToken);
        if (recoveryCase is null)
        {
            return Results.NotFound();
        }
        if (recoveryCase.ResolvedAt.HasValue)
        {
            return Results.Conflict();
        }
        if (workflow == "BuyNow")
        {
            var saga = await dbContext.BuyNowSagas.FindAsync([correlationId], cancellationToken);
            if (saga?.CurrentState != "ManualInterventionRequired")
            {
                return Results.Conflict();
            }
            await publisher.Publish(new RetryBuyNowSaga { CorrelationId = correlationId, AuctionId = recoveryCase.AuctionId }, cancellationToken);
        }
        else if (workflow == "AuctionCompletion")
        {
            var saga = await dbContext.AuctionCompletionSagas.FindAsync([correlationId], cancellationToken);
            if (saga is null)
            {
                return Results.NotFound();
            }
            if (step == "Notifications" && saga.OrderId.HasValue)
            {
                await publisher.Publish(new SendAuctionCompletionNotifications
                {
                    CorrelationId = saga.CorrelationId,
                    AuctionId = saga.AuctionId,
                    OrderId = saga.OrderId.Value,
                    SellerId = saga.SellerId,
                    SellerUsername = saga.SellerUsername,
                    WinnerId = saga.WinnerId,
                    WinnerUsername = saga.WinnerUsername,
                    ItemTitle = saga.ItemTitle,
                    Amount = saga.WinningBidAmount
                }, cancellationToken);
            }
            else if (saga.CurrentState == "ManualInterventionRequired")
            {
                await publisher.Publish(new RetryAuctionCompletionSaga
                {
                    CorrelationId = correlationId,
                    AuctionId = recoveryCase.AuctionId
                }, cancellationToken);
            }
            else
            {
                return Results.Conflict();
            }
        }
        else
        {
            return Results.BadRequest();
        }

        recoveryCase.LastRetriedAt = DateTimeOffset.UtcNow;
        recoveryCase.RetryRequests++;
        recoveryCase.RetriedBy = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Results.Accepted();
    }
}
