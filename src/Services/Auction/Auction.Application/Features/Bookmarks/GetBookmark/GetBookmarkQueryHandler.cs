using Auctions.Application.DTOs;
using Auctions.Application.Interfaces;
using Auctions.Domain.Entities;
using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace Auctions.Application.Features.Bookmarks.GetBookmark;

public class GetBookmarkQueryHandler : IQueryHandler<GetBookmarkQuery, BookmarkItemDto?>
{
    private readonly IBookmarkRepository _bookmarkRepository;
    private readonly ILogger<GetBookmarkQueryHandler> _logger;

    public GetBookmarkQueryHandler(
        IBookmarkRepository bookmarkRepository,
        ILogger<GetBookmarkQueryHandler> logger)
    {
        _bookmarkRepository = bookmarkRepository;
        _logger = logger;
    }

    public async Task<Result<BookmarkItemDto?>> Handle(GetBookmarkQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching bookmark for user {UserId} and auction {AuctionId}", 
            request.UserId, request.AuctionId);

        var bookmark = await _bookmarkRepository.GetByUserAndAuctionAsync(
            request.UserId, 
            request.AuctionId,
            BookmarkType.Watchlist,
            cancellationToken);

        if (bookmark == null)
        {
            return Result.Success<BookmarkItemDto?>(null);
        }

        var dto = new BookmarkItemDto
        {
            Id = bookmark.Id,
            AuctionId = bookmark.AuctionId,
            AddedAt = bookmark.AddedAt,
            NotifyOnBid = bookmark.NotifyOnBid,
            NotifyOnEnd = bookmark.NotifyOnEnd
        };

        return Result.Success<BookmarkItemDto?>(dto);
    }
}
