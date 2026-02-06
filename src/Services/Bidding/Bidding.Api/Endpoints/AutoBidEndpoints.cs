using Bidding.Application.Errors;
using Bidding.Application.Features.AutoBids.CancelAutoBid;
using Bidding.Application.Features.AutoBids.CreateAutoBid;
using Bidding.Application.Features.AutoBids.GetAutoBid;
using Bidding.Application.Features.AutoBids.GetMyAutoBids;
using Bidding.Application.Features.AutoBids.ToggleAutoBid;
using Bidding.Application.Features.AutoBids.UpdateAutoBid;
using Bidding.Domain.Constants;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Web.Authorization;
using BuildingBlocks.Web.Helpers;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Bidding.Api.Endpoints;

public class AutoBidEndpoints : ICarterModule
{
    private const string AutoBidResourceName = "AutoBid";

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
            .Produces<PaginatedResult<MyAutoBidDto>>(StatusCodes.Status200OK);

        group.MapPut("/{autoBidId:guid}", UpdateAutoBid)
            .WithName("UpdateAutoBid")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Bids.Place))
            .Produces<UpdateAutoBidResult>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapPost("/{autoBidId:guid}/toggle", ToggleAutoBid)
            .WithName("ToggleAutoBid")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Bids.Place))
            .Produces<ToggleAutoBidResult>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{autoBidId:guid}/cancel", CancelAutoBid)
            .WithName("CancelAutoBid")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Bids.Place))
            .Produces<CancelAutoBidResult>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> CreateAutoBid(
        CreateAutoBidDto request,
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

        if (!result.IsSuccess)
        {
            return Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
        }

        if (result.Value == null)
            return Results.BadRequest(ProblemDetailsHelper.FromError(BiddingErrors.AutoBid.CreateFailed("Auto-bid creation failed")));

        return Results.Created($"/api/v1/autobids/{result.Value.AutoBidId}", result.Value);
    }

    private static async Task<IResult> GetAutoBid(
        Guid autoBidId,
        IMediator mediator,
        CancellationToken ct)
    {
        var query = new GetAutoBidQuery(autoBidId);
        var result = await mediator.Send(query, ct);

        if (!result.IsSuccess || result.Value == null)
        {
            return Results.NotFound(ProblemDetailsHelper.NotFound(AutoBidResourceName, autoBidId));
        }

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> GetMyAutoBids(
        HttpContext context,
        IMediator mediator,
        [FromQuery] bool? activeOnly,
        [FromQuery] int page = BidDefaults.DefaultPage,
        [FromQuery] int pageSize = BidDefaults.DefaultPageSize,
        CancellationToken ct = default)
    {
        var userId = UserHelper.GetRequiredUserId(context.User);
        page = page < 1 ? BidDefaults.DefaultPage : page;
        pageSize = pageSize < 1 ? BidDefaults.DefaultPageSize : Math.Min(pageSize, BidDefaults.MaxPageSize);
        var query = new GetMyAutoBidsQuery(userId, IsActive: activeOnly, Page: page, PageSize: pageSize);
        var result = await mediator.Send(query, ct);

        if (!result.IsSuccess)
        {
            return Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
        }

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> UpdateAutoBid(
        Guid autoBidId,
        UpdateAutoBidDto request,
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

        if (!result.IsSuccess)
        {
            if (result.Error == BiddingErrors.AutoBid.NotFound)
                return Results.NotFound(ProblemDetailsHelper.NotFound(AutoBidResourceName, autoBidId));
            return Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
        }

        return Results.Ok(result.Value);
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

        if (!result.IsSuccess)
        {
            if (result.Error == BiddingErrors.AutoBid.NotFound)
                return Results.NotFound(ProblemDetailsHelper.NotFound(AutoBidResourceName, autoBidId));
            return Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
        }

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> ToggleAutoBid(
        Guid autoBidId,
        [FromBody] ToggleAutoBidDto request,
        HttpContext context,
        IMediator mediator,
        CancellationToken ct)
    {
        var userId = UserHelper.GetRequiredUserId(context.User);
        var command = new ToggleAutoBidCommand(autoBidId, userId, request.Activate);
        var result = await mediator.Send(command, ct);

        if (!result.IsSuccess)
        {
            if (result.Error == BiddingErrors.AutoBid.NotFound)
                return Results.NotFound(ProblemDetailsHelper.NotFound(AutoBidResourceName, autoBidId));
            return Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
        }

        return Results.Ok(result.Value);
    }
}

public record ToggleAutoBidDto(bool Activate);
