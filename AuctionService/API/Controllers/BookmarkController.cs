#nullable enable
using Asp.Versioning;
using AuctionService.Application.DTOs;
using AuctionService.Application.Interfaces;
using AuctionService.Domain.Entities;
using Common.Repository.Interfaces;
using Common.Utilities.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuctionService.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/bookmarks")]
[Authorize(Policy = "AuctionScope")]
public class BookmarkController : ControllerBase
{
    private readonly IUserAuctionBookmarkRepository _bookmarkRepository;
    private readonly IAuctionRepository _auctionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BookmarkController> _logger;

    public BookmarkController(
        IUserAuctionBookmarkRepository bookmarkRepository,
        IAuctionRepository auctionRepository,
        IUnitOfWork unitOfWork,
        ILogger<BookmarkController> logger)
    {
        _bookmarkRepository = bookmarkRepository;
        _auctionRepository = auctionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    [HttpGet("watchlist")]
    [ProducesResponseType(typeof(List<BookmarkItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<BookmarkItemDto>>> GetWatchlist(CancellationToken cancellationToken)
    {
        var username = UserHelper.GetUsername(User);
        var items = await _bookmarkRepository.GetByUsernameAsync(username, BookmarkType.Watchlist, cancellationToken);
        return Ok(MapToDto(items));
    }

    [HttpGet("wishlist")]
    [ProducesResponseType(typeof(List<BookmarkItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<BookmarkItemDto>>> GetWishlist(CancellationToken cancellationToken)
    {
        var username = UserHelper.GetUsername(User);
        var items = await _bookmarkRepository.GetByUsernameAsync(username, BookmarkType.Wishlist, cancellationToken);
        return Ok(MapToDto(items));
    }

    [HttpGet("watchlist/count")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<ActionResult<int>> GetWatchlistCount(CancellationToken cancellationToken)
    {
        var userId = UserHelper.GetRequiredUserId(User);
        var count = await _bookmarkRepository.GetCountAsync(userId, BookmarkType.Watchlist, cancellationToken);
        return Ok(count);
    }

    [HttpGet("wishlist/count")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<ActionResult<int>> GetWishlistCount(CancellationToken cancellationToken)
    {
        var userId = UserHelper.GetRequiredUserId(User);
        var count = await _bookmarkRepository.GetCountAsync(userId, BookmarkType.Wishlist, cancellationToken);
        return Ok(count);
    }

    [HttpGet("watchlist/check/{auctionId:guid}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<ActionResult<bool>> IsInWatchlist(Guid auctionId, CancellationToken cancellationToken)
    {
        var userId = UserHelper.GetRequiredUserId(User);
        var exists = await _bookmarkRepository.ExistsAsync(userId, auctionId, BookmarkType.Watchlist, cancellationToken);
        return Ok(exists);
    }

    [HttpGet("wishlist/check/{auctionId:guid}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<ActionResult<bool>> IsInWishlist(Guid auctionId, CancellationToken cancellationToken)
    {
        var userId = UserHelper.GetRequiredUserId(User);
        var exists = await _bookmarkRepository.ExistsAsync(userId, auctionId, BookmarkType.Wishlist, cancellationToken);
        return Ok(exists);
    }

    [HttpPost("watchlist")]
    [ProducesResponseType(typeof(BookmarkItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookmarkItemDto>> AddToWatchlist(
        [FromBody] AddBookmarkDto dto,
        CancellationToken cancellationToken)
    {
        return await AddBookmark(dto.AuctionId, BookmarkType.Watchlist, dto.NotifyOnBid, dto.NotifyOnEnd, cancellationToken);
    }

    [HttpPost("wishlist/{auctionId:guid}")]
    [ProducesResponseType(typeof(BookmarkItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookmarkItemDto>> AddToWishlist(Guid auctionId, CancellationToken cancellationToken)
    {
        return await AddBookmark(auctionId, BookmarkType.Wishlist, false, false, cancellationToken);
    }

    [HttpDelete("watchlist/{auctionId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> RemoveFromWatchlist(Guid auctionId, CancellationToken cancellationToken)
    {
        return await RemoveBookmark(auctionId, BookmarkType.Watchlist, cancellationToken);
    }

    [HttpDelete("wishlist/{auctionId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> RemoveFromWishlist(Guid auctionId, CancellationToken cancellationToken)
    {
        return await RemoveBookmark(auctionId, BookmarkType.Wishlist, cancellationToken);
    }

    [HttpPut("watchlist/{auctionId:guid}/notifications")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdateNotificationSettings(
        Guid auctionId,
        [FromBody] UpdateBookmarkNotificationsDto dto,
        CancellationToken cancellationToken)
    {
        var userId = UserHelper.GetRequiredUserId(User);
        var bookmark = await _bookmarkRepository.GetByUserAndAuctionAsync(userId, auctionId, BookmarkType.Watchlist, cancellationToken);

        if (bookmark == null)
        {
            return NotFound("Item not found in watchlist");
        }

        bookmark.NotifyOnBid = dto.NotifyOnBid;
        bookmark.NotifyOnEnd = dto.NotifyOnEnd;

        await _bookmarkRepository.UpdateAsync(bookmark, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpPost("wishlist/{auctionId:guid}/toggle")]
    [ProducesResponseType(typeof(BookmarkToggleResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<BookmarkToggleResponse>> ToggleWishlist(Guid auctionId, CancellationToken cancellationToken)
    {
        var userId = UserHelper.GetRequiredUserId(User);
        var username = UserHelper.GetUsername(User);

        var existing = await _bookmarkRepository.GetByUserAndAuctionAsync(userId, auctionId, BookmarkType.Wishlist, cancellationToken);

        if (existing != null)
        {
            await _bookmarkRepository.DeleteAsync(existing.Id, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Ok(new BookmarkToggleResponse { IsBookmarked = false, Message = "Removed from wishlist" });
        }

        var auction = await _auctionRepository.GetByIdAsync(auctionId, cancellationToken);
        if (auction == null)
        {
            return NotFound(new { message = "Auction not found" });
        }

        var bookmark = new UserAuctionBookmark
        {
            UserId = userId,
            Username = username,
            AuctionId = auctionId,
            Type = BookmarkType.Wishlist,
            AddedAt = DateTimeOffset.UtcNow
        };

        await _bookmarkRepository.AddAsync(bookmark, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Ok(new BookmarkToggleResponse { IsBookmarked = true, Message = "Added to wishlist" });
    }

    private async Task<ActionResult<BookmarkItemDto>> AddBookmark(
        Guid auctionId,
        BookmarkType type,
        bool notifyOnBid,
        bool notifyOnEnd,
        CancellationToken cancellationToken)
    {
        var userId = UserHelper.GetRequiredUserId(User);
        var username = UserHelper.GetUsername(User);

        var auction = await _auctionRepository.GetByIdAsync(auctionId, cancellationToken);
        if (auction == null)
        {
            return NotFound("Auction not found");
        }

        var existing = await _bookmarkRepository.ExistsAsync(userId, auctionId, type, cancellationToken);
        if (existing)
        {
            return BadRequest($"Auction is already in your {type.ToString().ToLower()}");
        }

        var bookmark = new UserAuctionBookmark
        {
            UserId = userId,
            Username = username,
            AuctionId = auctionId,
            Type = type,
            NotifyOnBid = notifyOnBid,
            NotifyOnEnd = notifyOnEnd,
            AddedAt = DateTimeOffset.UtcNow
        };

        await _bookmarkRepository.AddAsync(bookmark, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {Username} added auction {AuctionId} to {BookmarkType}", username, auctionId, type);

        var result = new BookmarkItemDto
        {
            Id = bookmark.Id,
            AuctionId = bookmark.AuctionId,
            BookmarkType = bookmark.Type.ToString(),
            AuctionTitle = auction.Item?.Title ?? string.Empty,
            ImageUrl = auction.Item?.Files.FirstOrDefault(f => f.IsPrimary)?.Url,
            CurrentBid = auction.CurrentHighBid ?? 0,
            ReservePrice = auction.ReservePrice,
            AuctionEnd = auction.AuctionEnd,
            Status = auction.Status.ToString(),
            AddedAt = bookmark.AddedAt,
            NotifyOnBid = bookmark.NotifyOnBid,
            NotifyOnEnd = bookmark.NotifyOnEnd
        };

        return CreatedAtAction(type == BookmarkType.Watchlist ? nameof(GetWatchlist) : nameof(GetWishlist), result);
    }

    private async Task<ActionResult> RemoveBookmark(Guid auctionId, BookmarkType type, CancellationToken cancellationToken)
    {
        var userId = UserHelper.GetRequiredUserId(User);
        var username = UserHelper.GetUsername(User);

        var bookmark = await _bookmarkRepository.GetByUserAndAuctionAsync(userId, auctionId, type, cancellationToken);

        if (bookmark == null)
        {
            return NotFound($"Item not found in {type.ToString().ToLower()}");
        }

        await _bookmarkRepository.DeleteAsync(bookmark.Id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {Username} removed auction {AuctionId} from {BookmarkType}", username, auctionId, type);

        return NoContent();
    }

    private static List<BookmarkItemDto> MapToDto(List<UserAuctionBookmark> items)
    {
        return items.Select(item => new BookmarkItemDto
        {
            Id = item.Id,
            AuctionId = item.AuctionId,
            BookmarkType = item.Type.ToString(),
            AuctionTitle = item.Auction?.Item?.Title ?? string.Empty,
            ImageUrl = item.Auction?.Item?.Files.FirstOrDefault(f => f.IsPrimary)?.Url,
            CurrentBid = item.Auction?.CurrentHighBid ?? 0,
            ReservePrice = item.Auction?.ReservePrice ?? 0,
            AuctionEnd = item.Auction?.AuctionEnd ?? DateTimeOffset.MinValue,
            Status = item.Auction?.Status.ToString() ?? string.Empty,
            AddedAt = item.AddedAt,
            NotifyOnBid = item.NotifyOnBid,
            NotifyOnEnd = item.NotifyOnEnd
        }).ToList();
    }
}

public class AddBookmarkDto
{
    public Guid AuctionId { get; set; }
    public bool NotifyOnBid { get; set; }
    public bool NotifyOnEnd { get; set; } = true;
}

public class UpdateBookmarkNotificationsDto
{
    public bool NotifyOnBid { get; set; }
    public bool NotifyOnEnd { get; set; }
}

public class BookmarkToggleResponse
{
    public bool IsBookmarked { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class BookmarkItemDto
{
    public Guid Id { get; set; }
    public Guid AuctionId { get; set; }
    public string BookmarkType { get; set; } = string.Empty;
    public string AuctionTitle { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public decimal CurrentBid { get; set; }
    public decimal ReservePrice { get; set; }
    public DateTimeOffset AuctionEnd { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset AddedAt { get; set; }
    public bool NotifyOnBid { get; set; }
    public bool NotifyOnEnd { get; set; }
}
