#nullable enable
using Auction.Application.Errors;
using Auctions.Application.Commands.Bookmarks.AddToWatchlist;
using Auctions.Application.Commands.Bookmarks.RemoveFromWatchlist;
using Auctions.Application.Commands.Bookmarks.UpdateBookmarkNotifications;
using Auctions.Application.DTOs;
using Auctions.Application.Queries.Bookmarks.GetWatchlist;
using Auctions.Application.Queries.Bookmarks.GetWatchlistCount;
using Auctions.Application.Queries.Bookmarks.IsInWatchlist;
using BuildingBlocks.Web.Authorization;
using BuildingBlocks.Web.Helpers;
using Carter;
using MediatR;

namespace Auctions.Api.Endpoints.Bookmarks;

public class BookmarkEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/bookmarks")
            .WithTags("Bookmarks")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Auctions.View))
            .WithOpenApi();

        group.MapGet("/watchlist", GetWatchlist)
            .WithName("GetWatchlist")
            .Produces<List<BookmarkItemDto>>(StatusCodes.Status200OK);

        group.MapGet("/watchlist/count", GetWatchlistCount)
            .WithName("GetWatchlistCount")
            .Produces<int>(StatusCodes.Status200OK);

        group.MapGet("/watchlist/check/{auctionId:guid}", IsInWatchlist)
            .WithName("IsInWatchlist")
            .Produces<bool>(StatusCodes.Status200OK);

        group.MapPost("/watchlist", AddToWatchlist)
            .WithName("AddToWatchlist")
            .Produces<BookmarkItemDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/watchlist/{auctionId:guid}", RemoveFromWatchlist)
            .WithName("RemoveFromWatchlist")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPut("/watchlist/{auctionId:guid}/notifications", UpdateNotificationSettings)
            .WithName("UpdateBookmarkNotifications")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GetWatchlist(
        HttpContext httpContext,
        ISender sender,
        CancellationToken ct)
    {
        var username = UserHelper.GetUsername(httpContext.User);
        var query = new GetWatchlistQuery(username);
        var result = await sender.Send(query, ct);
        return result.IsSuccess ? Results.Ok(result.Value) : ProblemDetailsHelper.FromError(result.Error);
    }

    private static async Task<IResult> GetWatchlistCount(
        HttpContext httpContext,
        ISender sender,
        CancellationToken ct)
    {
        var userId = UserHelper.GetRequiredUserId(httpContext.User);
        var query = new GetWatchlistCountQuery(userId);
        var result = await sender.Send(query, ct);
        return result.IsSuccess ? Results.Ok(result.Value) : ProblemDetailsHelper.FromError(result.Error);
    }

    private static async Task<IResult> IsInWatchlist(
        Guid auctionId,
        HttpContext httpContext,
        ISender sender,
        CancellationToken ct)
    {
        var userId = UserHelper.GetRequiredUserId(httpContext.User);
        var query = new IsInWatchlistQuery(userId, auctionId);
        var result = await sender.Send(query, ct);
        return result.IsSuccess ? Results.Ok(result.Value) : ProblemDetailsHelper.FromError(result.Error);
    }

    private static async Task<IResult> AddToWatchlist(
        AddBookmarkDto dto,
        HttpContext httpContext,
        ISender sender,
        CancellationToken ct)
    {
        var userId = UserHelper.GetRequiredUserId(httpContext.User);
        var username = UserHelper.GetUsername(httpContext.User);

        var command = new AddToWatchlistCommand(
            dto.AuctionId,
            userId,
            username,
            dto.NotifyOnBid,
            dto.NotifyOnEnd);

        var result = await sender.Send(command, ct);

        if (result.IsFailure)
        {
            if (result.Error.Code == AuctionErrors.Auction.NotFound.Code)
                return Results.NotFound(result.Error.Message);
            return ProblemDetailsHelper.FromError(result.Error);
        }

        return Results.CreatedAtRoute("GetWatchlist", result.Value);
    }

    private static async Task<IResult> RemoveFromWatchlist(
        Guid auctionId,
        HttpContext httpContext,
        ISender sender,
        CancellationToken ct)
    {
        var userId = UserHelper.GetRequiredUserId(httpContext.User);
        var username = UserHelper.GetUsername(httpContext.User);

        var command = new RemoveFromWatchlistCommand(auctionId, userId, username);
        var result = await sender.Send(command, ct);

        if (result.IsFailure)
        {
            if (result.Error.Code == AuctionErrors.Bookmark.NotFound.Code)
                return Results.NotFound(result.Error.Message);
            return ProblemDetailsHelper.FromError(result.Error);
        }

        return Results.NoContent();
    }

    private static async Task<IResult> UpdateNotificationSettings(
        Guid auctionId,
        UpdateBookmarkNotificationsDto dto,
        HttpContext httpContext,
        ISender sender,
        CancellationToken ct)
    {
        var userId = UserHelper.GetRequiredUserId(httpContext.User);

        var command = new UpdateBookmarkNotificationsCommand(
            auctionId,
            userId,
            dto.NotifyOnBid,
            dto.NotifyOnEnd);

        var result = await sender.Send(command, ct);

        if (result.IsFailure)
        {
            if (result.Error.Code == AuctionErrors.Bookmark.NotFound.Code)
                return Results.NotFound(result.Error.Message);
            return ProblemDetailsHelper.FromError(result.Error);
        }

        return Results.NoContent();
    }
}

