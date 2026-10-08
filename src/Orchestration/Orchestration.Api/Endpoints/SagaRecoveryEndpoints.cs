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
        group.MapGet("/", async (OrchestrationDbContext db, CancellationToken ct) =>
            Results.Ok(await db.RecoveryCases.AsNoTracking().Where(x => x.ResolvedAt == null)
                .OrderBy(x => x.CreatedAt).Take(100).ToListAsync(ct)));
        group.MapPost("/{workflow}/{correlationId:guid}/{step}/retry", async (string workflow, Guid correlationId, string step,
            OrchestrationDbContext db, IPublishEndpoint publish, ClaimsPrincipal user, CancellationToken ct) =>
        {
            await using var tx = await db.Database.BeginTransactionAsync(ct);
            await db.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock(hashtextextended({workflow + correlationId}, 0))", ct);
            var record = await db.RecoveryCases.FindAsync([workflow, correlationId, step], ct);
            if (record is null) return Results.NotFound();
            if (record.ResolvedAt.HasValue) return Results.Conflict();
            if (workflow == "BuyNow")
            {
                var saga = await db.BuyNowSagas.FindAsync([correlationId], ct);
                if (saga?.CurrentState != "ManualInterventionRequired") return Results.Conflict();
                await publish.Publish(new RetryBuyNowSaga { CorrelationId = correlationId, AuctionId = record.AuctionId }, ct);
            }
            else if (workflow == "AuctionCompletion")
            {
                var saga = await db.AuctionCompletionSagas.FindAsync([correlationId], ct);
                if (saga is null) return Results.NotFound();
                if (step == "Notifications" && saga.OrderId.HasValue)
                    await publish.Publish(new SendAuctionCompletionNotifications
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
                    }, ct);
                else if (saga.CurrentState == "ManualInterventionRequired")
                    await publish.Publish(new RetryAuctionCompletionSaga { CorrelationId = correlationId, AuctionId = record.AuctionId }, ct);
                else return Results.Conflict();
            }
            else return Results.BadRequest();
            record.LastRetriedAt = DateTimeOffset.UtcNow;
            record.RetryRequests++;
            record.RetriedBy = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return Results.Accepted();
        });
    }
}
