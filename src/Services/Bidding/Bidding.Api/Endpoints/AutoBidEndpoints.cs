using Bidding.Application.Features.AutoBids.CancelAutoBid;
using Bidding.Application.Features.AutoBids.CreateAutoBid;
using Bidding.Application.Features.AutoBids.GetAutoBid;
using Bidding.Application.Features.AutoBids.GetMyAutoBids;
using Bidding.Application.Features.AutoBids.UpdateAutoBid;
using BuildingBlocks.Web.Authorization;
using BuildingBlocks.Web.Helpers;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Bidding.Api.Endpoints;

public class AutoBidEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/autobids")
            .WithTags("AutoBids")
            .WithOpenApi();

        group.MapPost("/", CreateAutoBid)
            .WithName("CreateAutoBid")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Bids.Place))
            .Produces<CreateAutoBidResult>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapGet("/{autoBidId:guid}", GetAutoBid)
            .WithName("GetAutoBid")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Bids.View))
            .Produces<AutoBidDetailDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/my", GetMyAutoBids)
            .WithName("GetMyAutoBids")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Bids.View))
            .Produces<MyAutoBidsResult>(StatusCodes.Status200OK);

        group.MapPut("/{autoBidId:guid}", UpdateAutoBid)
            .WithName("UpdateAutoBid")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Bids.Place))
            .Produces<UpdateAutoBidResult>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapPost("/{autoBidId:guid}/cancel", CancelAutoBid)
            .WithName("CancelAutoBid")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Bids.Place))
            .Produces<CancelAutoBidResult>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> CreateAutoBid(
        CreateAutoBidRequest request,
        HttpContext context,
        IMediator mediator,
        CancellationToken ct)
    {
        var userId = UserHelper.GetRequiredUserId(context.User);
        var username = UserHelper.GetUsername(context.User);

        var command = new CreateAutoBidCommand(
            request.AuctionId,
            userId,
            username,
            request.MaxAmount,
            request.BidIncrement);

        var result = await mediator.Send(command, ct);

        return result.IsSuccess
            ? Results.Created($"/api/v1/autobids/{result.Value.AutoBidId}", result.Value)
            : Results.BadRequest(new { error = result.Error.Message });
    }

    private static async Task<IResult> GetAutoBid(
        Guid autoBidId,
        IMediator mediator,
        CancellationToken ct)
    {
        var query = new GetAutoBidQuery(autoBidId);
        var result = await mediator.Send(query, ct);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.NotFound();
    }

    private static async Task<IResult> GetMyAutoBids(
        HttpContext context,
        IMediator mediator,
        [FromQuery] bool? activeOnly,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var userId = UserHelper.GetRequiredUserId(context.User);
        var query = new GetMyAutoBidsQuery(userId, activeOnly, page, pageSize);
        var result = await mediator.Send(query, ct);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(new { error = result.Error.Message });
    }

    private static async Task<IResult> UpdateAutoBid(
        Guid autoBidId,
        UpdateAutoBidRequest request,
        HttpContext context,
        IMediator mediator,
        CancellationToken ct)
    {
        var userId = UserHelper.GetRequiredUserId(context.User);

        var command = new UpdateAutoBidCommand(
            autoBidId,
            userId,
            request.MaxAmount);

        var result = await mediator.Send(command, ct);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(new { error = result.Error.Message });
    }

    private static async Task<IResult> CancelAutoBid(
        Guid autoBidId,
        HttpContext context,
        IMediator mediator,
        CancellationToken ct)
    {
        var userId = UserHelper.GetRequiredUserId(context.User);
        var command = new CancelAutoBidCommand(autoBidId, userId);
        var result = await mediator.Send(command, ct);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(new { error = result.Error.Message });
    }
}

public record CreateAutoBidRequest
{
    public Guid AuctionId { get; init; }
    public decimal MaxAmount { get; init; }
    public decimal? BidIncrement { get; init; }
}

public record UpdateAutoBidRequest
{
    public decimal MaxAmount { get; init; }
    public decimal? BidIncrement { get; init; }
}
