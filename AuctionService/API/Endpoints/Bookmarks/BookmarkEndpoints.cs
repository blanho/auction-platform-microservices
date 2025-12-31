#nullable enable
using AuctionService.Application.Commands.Bookmarks.AddToWatchlist;
using AuctionService.Application.Commands.Bookmarks.RemoveFromWatchlist;
using AuctionService.Application.Commands.Bookmarks.UpdateBookmarkNotifications;
using AuctionService.Application.DTOs;
using AuctionService.Application.Queries.Bookmarks.GetWatchlist;
using AuctionService.Application.Queries.Bookmarks.GetWatchlistCount;
using AuctionService.Application.Queries.Bookmarks.IsInWatchlist;
using Carter;
using Common.Utilities.Helpers;
using MediatR;

namespace AuctionService.API.Endpoints.Bookmarks;

public class BookmarkEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/bookmarks")
            .WithTags("Bookmarks")
            .RequireAuthorization("AuctionScope")
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
        return result.IsSuccess ? Results.Ok(result.Value) : Results.Problem(result.Error.Message);
    }

    private static async Task<IResult> GetWatchlistCount(
        HttpContext httpContext,
        ISender sender,
        CancellationToken ct)
    {
        var userId = UserHelper.GetRequiredUserId(httpContext.User);
        var query = new GetWatchlistCountQuery(userId);
        var result = await sender.Send(query, ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Results.Problem(result.Error.Message);
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
        return result.IsSuccess ? Results.Ok(result.Value) : Results.Problem(result.Error.Message);
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
            return result.Error.Code switch
            {
                "Auction.NotFound" => Results.NotFound(result.Error.Message),
                "Bookmark.AlreadyExists" => Results.BadRequest(result.Error.Message),
                _ => Results.Problem(result.Error.Message)
            };
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
            return Results.NotFound(result.Error.Message);

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
            return Results.NotFound(result.Error.Message);

        return Results.NoContent();
    }
}
