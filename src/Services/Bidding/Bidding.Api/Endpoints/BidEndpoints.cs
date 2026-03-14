using Bidding.Application.DTOs;
using Bidding.Application.Features.Bids.GetBidById;
using Bidding.Application.Features.Bids.GetBidHistory;
using Bidding.Application.Features.Bids.GetBidIncrement;
using Bidding.Application.Features.Bids.GetBidStats;
using Bidding.Application.Features.Bids.GetWinningBids;
using Bidding.Application.Features.Bids.PlaceBid;
using Bidding.Application.Features.Bids.GetBidsForAuction;
using Bidding.Application.Features.Bids.GetMyBids;
using Bidding.Application.Features.Bids.RetractBid;
using Bidding.Domain.Constants;
using Bidding.Domain.Enums;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Web.Authorization;
using BuildingBlocks.Web.Extensions;
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
            .Produces<PaginatedResult<WinningBidDto>>(StatusCodes.Status200OK);

        group.MapGet("/history", GetBidHistory)
            .WithName("GetBidHistory")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Bids.View))
            .Produces<PaginatedResult<BidHistoryItemDto>>(StatusCodes.Status200OK);

        group.MapPost("/{bidId:guid}/retract", RetractBid)
            .WithName("RetractBid")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Bids.Place))
            .Produces<RetractBidResult>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapGet("/increment/{currentBid:decimal}", GetBidIncrementInfo)
            .WithName("GetBidIncrementInfo")
            .Produces<BidIncrementInfoDto>(StatusCodes.Status200OK);

        group.MapGet("/stats", GetBidStats)
            .WithName("GetBidStats")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Bids.View))
            .Produces<BidStatsResponse>(StatusCodes.Status200OK);
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

        return result.ToApiResult(bid => 
            Results.Created($"/api/v1/bids/auction/{dto.AuctionId}", bid));
    }

    private static async Task<IResult> GetBidById(
        Guid bidId,
        IMediator mediator,
        CancellationToken ct)
    {
        var query = new GetBidByIdQuery(bidId);
        var result = await mediator.Send(query, ct);

        return result.ToOkResult();
    }

    private static async Task<IResult> GetBidsForAuction(
        Guid auctionId,
        IMediator mediator,
        CancellationToken ct)
    {
        var query = new GetBidsForAuctionQuery(auctionId);
        var result = await mediator.Send(query, ct);
        return result.ToOkResult();
    }

    private static async Task<IResult> GetMyBids(
        HttpContext context,
        IMediator mediator,
        CancellationToken ct)
    {
        var username = UserHelper.GetUsername(context.User);
        var query = new GetMyBidsQuery(username);
        var result = await mediator.Send(query, ct);
        return result.ToOkResult();
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
        var query = new GetWinningBidsQuery(userId, Page: page, PageSize: pageSize);
        var result = await mediator.Send(query, ct);
        return result.ToOkResult();
    }

    private static async Task<IResult> GetBidHistory(
        HttpContext context,
        IMediator mediator,
        [AsParameters] BidHistoryFilterRequest filter,
        CancellationToken ct = default)
    {
        var userId = UserHelper.GetRequiredUserId(context.User);
        var page = filter.Page < 1 ? BidDefaults.DefaultPage : filter.Page;
        var pageSize = filter.PageSize < 1 ? BidDefaults.DefaultPageSize : Math.Min(filter.PageSize, BidDefaults.MaxPageSize);
        var query = new GetBidHistoryQuery(
            AuctionId: filter.AuctionId,
            UserId: userId,
            Status: filter.Status,
            FromDate: filter.FromDate,
            ToDate: filter.ToDate,
            Page: page,
            PageSize: pageSize);
        var result = await mediator.Send(query, ct);
        return result.ToOkResult();
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
        return result.ToOkResult();
    }

    private static async Task<IResult> GetBidIncrementInfo(
        decimal currentBid,
        IMediator mediator,
        CancellationToken ct)
    {
        var query = new GetBidIncrementQuery(currentBid);
        var result = await mediator.Send(query, ct);
        return result.ToOkResult();
    }

    private static async Task<IResult> GetBidStats(
        HttpContext context,
        IMediator mediator,
        [FromQuery] int days = BidDefaults.DefaultDaysForStats,
        [FromQuery] int topLimit = BidDefaults.DefaultTopBiddersLimit,
        CancellationToken ct = default)
    {
        var username = UserHelper.GetUsername(context.User);
        var query = new GetBidStatsQuery(username, days, topLimit);
        var result = await mediator.Send(query, ct);
        return result.ToOkResult();
    }
}
