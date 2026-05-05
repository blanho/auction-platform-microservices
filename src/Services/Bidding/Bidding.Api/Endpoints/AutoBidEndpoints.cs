using Bidding.Application.DTOs;
using Bidding.Application.Features.AutoBids.CancelAutoBid;
using Bidding.Application.Features.AutoBids.CreateAutoBid;
using Bidding.Application.Features.AutoBids.GetAutoBid;
using Bidding.Application.Features.AutoBids.GetAutoBidForAuction;
using Bidding.Application.Features.AutoBids.GetMyAutoBids;
using Bidding.Application.Features.AutoBids.ToggleAutoBid;
using Bidding.Application.Features.AutoBids.UpdateAutoBid;
using Bidding.Domain.Constants;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Web.Authorization;
using BuildingBlocks.Web.Extensions;
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

        group.MapGet("/auction/{auctionId:guid}", GetAutoBidForAuction)
            .WithName("GetAutoBidForAuction")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Bids.View))
            .Produces<AutoBidDetailDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent);

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

        return result.ToApiResult(value =>
            Results.Created($"/api/v1/autobids/{value.AutoBidId}", value));
    }

    private static async Task<IResult> GetAutoBid(
        Guid autoBidId,
        IMediator mediator,
        CancellationToken ct)
    {
        var query = new GetAutoBidQuery(autoBidId);
        var result = await mediator.Send(query, ct);
        return result.ToOkResult();
    }

    private static async Task<IResult> GetAutoBidForAuction(
        Guid auctionId,
        HttpContext context,
        IMediator mediator,
        CancellationToken ct)
    {
        var userId = UserHelper.GetRequiredUserId(context.User);
        var query = new GetAutoBidForAuctionQuery(auctionId, userId);
        var result = await mediator.Send(query, ct);
        return result.ToOkResult();
    }

    private static async Task<IResult> GetMyAutoBids(
        HttpContext context,
        IMediator mediator,
        [AsParameters] AutoBidsFilterRequest filter,
        CancellationToken ct = default)
    {
        var userId = UserHelper.GetRequiredUserId(context.User);
        var page = filter.Page < 1 ? BidDefaults.DefaultPage : filter.Page;
        var pageSize = filter.PageSize < 1 ? BidDefaults.DefaultPageSize : Math.Min(filter.PageSize, BidDefaults.MaxPageSize);
        var query = new GetMyAutoBidsQuery(userId, IsActive: filter.ActiveOnly, Page: page, PageSize: pageSize);
        var result = await mediator.Send(query, ct);
        return result.ToOkResult();
    }

    private static async Task<IResult> UpdateAutoBid(
        Guid autoBidId,
        UpdateAutoBidDto request,
        HttpContext context,
        IMediator mediator,
        CancellationToken ct)
    {
        var userId = UserHelper.GetRequiredUserId(context.User);
        var command = new UpdateAutoBidCommand(autoBidId, userId, request.MaxAmount);
        var result = await mediator.Send(command, ct);
        return result.ToOkResult();
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
        return result.ToOkResult();
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
        return result.ToOkResult();
    }
}
