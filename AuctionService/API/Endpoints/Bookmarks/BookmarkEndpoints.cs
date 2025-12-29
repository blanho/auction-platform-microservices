#nullable enable
using AuctionService.Application.DTOs;
using AuctionService.Application.Interfaces;
using AuctionService.Domain.Entities;
using Carter;
using Common.Repository.Interfaces;
using Common.Utilities.Helpers;

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
        IUserAuctionBookmarkRepository bookmarkRepository,
        CancellationToken ct)
    {
        var username = UserHelper.GetUsername(httpContext.User);
        var items = await bookmarkRepository.GetByUsernameAsync(username, BookmarkType.Watchlist, ct);
        return Results.Ok(MapToDto(items));
    }

    private static async Task<IResult> GetWatchlistCount(
        HttpContext httpContext,
        IUserAuctionBookmarkRepository bookmarkRepository,
        CancellationToken ct)
    {
        var userId = UserHelper.GetRequiredUserId(httpContext.User);
        var count = await bookmarkRepository.GetCountAsync(userId, BookmarkType.Watchlist, ct);
        return Results.Ok(count);
    }

    private static async Task<IResult> IsInWatchlist(
        Guid auctionId,
        HttpContext httpContext,
        IUserAuctionBookmarkRepository bookmarkRepository,
        CancellationToken ct)
    {
        var userId = UserHelper.GetRequiredUserId(httpContext.User);
        var exists = await bookmarkRepository.ExistsAsync(userId, auctionId, BookmarkType.Watchlist, ct);
        return Results.Ok(exists);
    }

    private static async Task<IResult> AddToWatchlist(
        AddBookmarkDto dto,
        HttpContext httpContext,
        IUserAuctionBookmarkRepository bookmarkRepository,
        IAuctionRepository auctionRepository,
        IUnitOfWork unitOfWork,
        ILogger<BookmarkEndpoints> logger,
        CancellationToken ct)
    {
        var userId = UserHelper.GetRequiredUserId(httpContext.User);
        var username = UserHelper.GetUsername(httpContext.User);

        var auction = await auctionRepository.GetByIdAsync(dto.AuctionId, ct);
        if (auction == null)
            return Results.NotFound("Auction not found");

        var existing = await bookmarkRepository.ExistsAsync(userId, dto.AuctionId, BookmarkType.Watchlist, ct);
        if (existing)
            return Results.BadRequest("Auction is already in your watchlist");

        var bookmark = new UserAuctionBookmark
        {
            UserId = userId,
            Username = username,
            AuctionId = dto.AuctionId,
            Type = BookmarkType.Watchlist,
            NotifyOnBid = dto.NotifyOnBid,
            NotifyOnEnd = dto.NotifyOnEnd,
            AddedAt = DateTimeOffset.UtcNow
        };

        await bookmarkRepository.AddAsync(bookmark, ct);
        await unitOfWork.SaveChangesAsync(ct);

        logger.LogInformation("User {Username} added auction {AuctionId} to watchlist", username, dto.AuctionId);

        var result = new BookmarkItemDto
        {
            Id = bookmark.Id,
            AuctionId = bookmark.AuctionId,
            BookmarkType = bookmark.Type.ToString(),
            AuctionTitle = auction.Item?.Title ?? string.Empty,
            PrimaryImageFileId = auction.Item?.Files.FirstOrDefault(f => f.IsPrimary)?.FileId,
            CurrentBid = auction.CurrentHighBid ?? 0,
            ReservePrice = auction.ReservePrice,
            AuctionEnd = auction.AuctionEnd,
            Status = auction.Status.ToString(),
            AddedAt = bookmark.AddedAt,
            NotifyOnBid = bookmark.NotifyOnBid,
            NotifyOnEnd = bookmark.NotifyOnEnd
        };

        return Results.CreatedAtRoute("GetWatchlist", result);
    }

    private static async Task<IResult> RemoveFromWatchlist(
        Guid auctionId,
        HttpContext httpContext,
        IUserAuctionBookmarkRepository bookmarkRepository,
        IUnitOfWork unitOfWork,
        ILogger<BookmarkEndpoints> logger,
        CancellationToken ct)
    {
        var userId = UserHelper.GetRequiredUserId(httpContext.User);
        var username = UserHelper.GetUsername(httpContext.User);

        var bookmark = await bookmarkRepository.GetByUserAndAuctionAsync(userId, auctionId, BookmarkType.Watchlist, ct);
        if (bookmark == null)
            return Results.NotFound("Item not found in watchlist");

        await bookmarkRepository.DeleteAsync(bookmark.Id, ct);
        await unitOfWork.SaveChangesAsync(ct);

        logger.LogInformation("User {Username} removed auction {AuctionId} from watchlist", username, auctionId);

        return Results.NoContent();
    }

    private static async Task<IResult> UpdateNotificationSettings(
        Guid auctionId,
        UpdateBookmarkNotificationsDto dto,
        HttpContext httpContext,
        IUserAuctionBookmarkRepository bookmarkRepository,
        IUnitOfWork unitOfWork,
        CancellationToken ct)
    {
        var userId = UserHelper.GetRequiredUserId(httpContext.User);

        var bookmark = await bookmarkRepository.GetByUserAndAuctionAsync(userId, auctionId, BookmarkType.Watchlist, ct);
        if (bookmark == null)
            return Results.NotFound("Item not found in watchlist");

        bookmark.NotifyOnBid = dto.NotifyOnBid;
        bookmark.NotifyOnEnd = dto.NotifyOnEnd;

        await bookmarkRepository.UpdateAsync(bookmark, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Results.NoContent();
    }

    private static List<BookmarkItemDto> MapToDto(IEnumerable<UserAuctionBookmark> items)
    {
        return items.Select(b => new BookmarkItemDto
        {
            Id = b.Id,
            AuctionId = b.AuctionId,
            BookmarkType = b.Type.ToString(),
            AuctionTitle = b.Auction?.Item?.Title ?? string.Empty,
            PrimaryImageFileId = b.Auction?.Item?.Files.FirstOrDefault(f => f.IsPrimary)?.FileId,
            CurrentBid = b.Auction?.CurrentHighBid ?? 0,
            ReservePrice = b.Auction?.ReservePrice ?? 0,
            AuctionEnd = b.Auction?.AuctionEnd ?? DateTimeOffset.MinValue,
            Status = b.Auction?.Status.ToString() ?? string.Empty,
            AddedAt = b.AddedAt,
            NotifyOnBid = b.NotifyOnBid,
            NotifyOnEnd = b.NotifyOnEnd
        }).ToList();
    }
}
