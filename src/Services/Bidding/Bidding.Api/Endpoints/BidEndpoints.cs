using Bidding.Application.DTOs;
using Bidding.Application.Errors;
using Bidding.Application.Features.Bids.GetBidById;
using Bidding.Application.Features.Bids.GetBidHistory;
using Bidding.Application.Features.Bids.GetWinningBids;
using Bidding.Application.Features.Bids.Commands.PlaceBid;
using Bidding.Application.Features.Bids.Queries.GetBidsForAuction;
using Bidding.Application.Features.Bids.Queries.GetMyBids;
using Bidding.Application.Features.Bids.RetractBid;
using Bidding.Domain.Constants;
using Bidding.Domain.Enums;
using Bidding.Domain.ValueObjects;
using BuildingBlocks.Web.Authorization;
using BuildingBlocks.Web.Helpers;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Bidding.Api.Endpoints;

public class BidEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/bids")
            .WithTags("Bids")
            .WithOpenApi();

        group.MapPost("/", PlaceBid)
            .WithName("PlaceBid")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Bids.Place))
            .RequireRateLimiting("BidRateLimit")
            .Produces<BidDto>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapGet("/{bidId:guid}", GetBidById)
            .WithName("GetBidById")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Bids.View))
            .Produces<BidDetailDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/auction/{auctionId:guid}", GetBidsForAuction)
            .WithName("GetBidsForAuction")
            .Produces<List<BidDto>>(StatusCodes.Status200OK);

        group.MapGet("/my", GetMyBids)
            .WithName("GetMyBids")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Bids.View))
            .Produces<List<BidDto>>(StatusCodes.Status200OK);

        group.MapGet("/winning", GetWinningBids)
            .WithName("GetWinningBids")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Bids.View))
            .Produces<PagedResult<WinningBidDto>>(StatusCodes.Status200OK);

        group.MapGet("/history", GetBidHistory)
            .WithName("GetBidHistory")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Bids.View))
            .Produces<BidHistoryResult>(StatusCodes.Status200OK);

        group.MapPost("/{bidId:guid}/retract", RetractBid)
            .WithName("RetractBid")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Bids.Place))
            .Produces<RetractBidResult>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapGet("/increment/{currentBid:decimal}", GetBidIncrementInfo)
            .WithName("GetBidIncrementInfo")
            .Produces<BidIncrementInfoDto>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> PlaceBid(
        PlaceBidDto dto,
        HttpContext context,
        IMediator mediator,
        CancellationToken ct)
    {
        var bidderId = UserHelper.GetRequiredUserId(context.User);
        var bidderUsername = UserHelper.GetUsername(context.User);

        var command = new PlaceBidCommand(
            dto.AuctionId,
            dto.Amount,
            bidderId,
            bidderUsername,
            Guid.NewGuid().ToString());

        var result = await mediator.Send(command, ct);

        if (!result.IsSuccess)
        {
            return Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
        }

        return Results.Created($"/api/v1/bids/auction/{dto.AuctionId}", result.Value);
    }

    private static async Task<IResult> GetBidById(
        Guid bidId,
        IMediator mediator,
        CancellationToken ct)
    {
        var query = new GetBidByIdQuery(bidId);
        var result = await mediator.Send(query, ct);

        if (!result.IsSuccess || result.Value == null)
        {
            return Results.NotFound(ProblemDetailsHelper.NotFound("Bid", bidId));
        }

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> GetBidsForAuction(
        Guid auctionId,
        IMediator mediator,
        CancellationToken ct)
    {
        var query = new GetBidsForAuctionQuery(auctionId);
        var result = await mediator.Send(query, ct);
        return Results.Ok(result.Value);
    }

    private static async Task<IResult> GetMyBids(
        HttpContext context,
        IMediator mediator,
        CancellationToken ct)
    {
        var username = UserHelper.GetUsername(context.User);
        var query = new GetMyBidsQuery(username);
        var result = await mediator.Send(query, ct);
        return Results.Ok(result.Value);
    }

    private static async Task<IResult> GetWinningBids(
        HttpContext context,
        IMediator mediator,
        [FromQuery] int page = BidDefaults.DefaultPage,
        [FromQuery] int pageSize = BidDefaults.DefaultPageSize,
        CancellationToken ct = default)
    {
        var userId = UserHelper.GetRequiredUserId(context.User);
        page = page < 1 ? BidDefaults.DefaultPage : page;
        pageSize = pageSize < 1 ? BidDefaults.DefaultPageSize : Math.Min(pageSize, BidDefaults.MaxPageSize);
        var query = new GetWinningBidsQuery(userId, page, pageSize);
        var result = await mediator.Send(query, ct);

        if (!result.IsSuccess)
        {
            return Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
        }

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> GetBidHistory(
        HttpContext context,
        IMediator mediator,
        [FromQuery] Guid? auctionId,
        [FromQuery] BidStatus? status,
        [FromQuery] DateTimeOffset? fromDate,
        [FromQuery] DateTimeOffset? toDate,
        [FromQuery] int page = BidDefaults.DefaultPage,
        [FromQuery] int pageSize = BidDefaults.DefaultPageSize,
        CancellationToken ct = default)
    {
        var userId = UserHelper.GetRequiredUserId(context.User);
        page = page < 1 ? BidDefaults.DefaultPage : page;
        pageSize = pageSize < 1 ? BidDefaults.DefaultPageSize : Math.Min(pageSize, BidDefaults.MaxPageSize);
        var query = new GetBidHistoryQuery(userId, auctionId, status, fromDate, toDate, page, pageSize);
        var result = await mediator.Send(query, ct);

        if (!result.IsSuccess)
        {
            return Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
        }

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> RetractBid(
        Guid bidId,
        HttpContext context,
        IMediator mediator,
        [FromBody] RetractBidDto request,
        CancellationToken ct)
    {
        var userId = UserHelper.GetRequiredUserId(context.User);
        var command = new RetractBidCommand(bidId, userId, request.Reason);
        var result = await mediator.Send(command, ct);

        if (!result.IsSuccess)
        {
            if (result.Error == BiddingErrors.Bid.NotFound)
                return Results.NotFound(ProblemDetailsHelper.NotFound("Bid", bidId));
            return Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
        }

        return Results.Ok(result.Value);
    }

    private static IResult GetBidIncrementInfo(decimal currentBid)
    {
        var minimumIncrement = BidIncrement.GetMinimumIncrement(currentBid);
        var minimumNextBid = BidIncrement.GetMinimumNextBid(currentBid);

        return Results.Ok(new BidIncrementInfoDto
        {
            CurrentBid = currentBid,
            MinimumIncrement = minimumIncrement,
            MinimumNextBid = minimumNextBid
        });
    }
}
