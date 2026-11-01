using Auctions.Application.Features.Auctions.BuyNow;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Auctions.Api.Endpoints.Auctions;

public sealed class AuctionPurchaseEndpoints : ICarterModule
{
    private const string IdempotencyKeyHeader = "Idempotency-Key";

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/auctions")
            .WithTags("Auctions")
            .WithOpenApi();

        group.MapGet("/{id:guid}/purchase", GetMyBuyNowPurchase)
            .RequireAuthorization()
            .WithName("GetMyBuyNowPurchase");

        group.MapPost("/{id:guid}/buy-now", BuyNow)
            .WithName("BuyNow")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Auctions.View))
            .Produces<BuyNowResultDto>(StatusCodes.Status202Accepted)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);
    }

    private static async Task<IResult> GetMyBuyNowPurchase(
        Guid id,
        HttpContext httpContext,
        IBuyNowPurchaseRepository purchases,
        CancellationToken ct)
    {
        var buyerId = UserHelper.GetRequiredUserId(httpContext.User);
        var purchase = await purchases.GetLatestAsync(id, buyerId, ct);
        return Results.Ok(purchase is null ? null : BuyNowResultDto.FromPurchase(purchase));
    }

    private static async Task<IResult> BuyNow(
        Guid id,
        HttpContext httpContext,
        IMediator mediator,
        CancellationToken ct)
    {
        var buyerId = UserHelper.GetRequiredUserId(httpContext.User);
        var buyerUsername = UserHelper.GetUsername(httpContext.User);

        Guid? correlationId = null;
        if (httpContext.Request.Headers.TryGetValue(IdempotencyKeyHeader, out var key))
        {
            if (!Guid.TryParse(key, out var parsed) || parsed == Guid.Empty)
            {
                return Results.BadRequest(ProblemDetailsHelper.ValidationError("Idempotency-Key must be a non-empty UUID"));
            }

            correlationId = parsed;
        }
        var command = new BuyNowCommand(id, buyerId, buyerUsername, correlationId);
        var result = await mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            return Results.Accepted($"/api/v1/auctions/{id}/purchase", result.Value);
        }

        var isConflict = result.Error!.Code.StartsWith("BuyNow.Conflict");
        return isConflict
            ? Results.Conflict(ProblemDetailsHelper.FromError(result.Error))
            : Results.BadRequest(ProblemDetailsHelper.FromError(result.Error));
    }
}
