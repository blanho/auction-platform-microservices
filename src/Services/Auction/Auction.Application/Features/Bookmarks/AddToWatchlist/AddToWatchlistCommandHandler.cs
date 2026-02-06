using Auctions.Application.Errors;
using Auctions.Application.DTOs;
using Auctions.Domain.Entities;
using Microsoft.Extensions.Logging;
// using BuildingBlocks.Infrastructure.Caching; // Use BuildingBlocks.Application.Abstractions instead
// using BuildingBlocks.Infrastructure.Repository; // Use BuildingBlocks.Application.Abstractions instead

namespace Auctions.Application.Commands.Bookmarks.AddToWatchlist;

public class AddToWatchlistCommandHandler : ICommandHandler<AddToWatchlistCommand, BookmarkItemDto>
{
    private readonly IBookmarkRepository _bookmarkRepository;
    private readonly IAuctionReadRepository _auctionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AddToWatchlistCommandHandler> _logger;

    public AddToWatchlistCommandHandler(
        IBookmarkRepository bookmarkRepository,
        IAuctionReadRepository auctionRepository,
        IUnitOfWork unitOfWork,
        ILogger<AddToWatchlistCommandHandler> logger)
    {
        _bookmarkRepository = bookmarkRepository;
        _auctionRepository = auctionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<BookmarkItemDto>> Handle(AddToWatchlistCommand request, CancellationToken cancellationToken)
    {
        var auction = await _auctionRepository.GetByIdAsync(request.AuctionId, cancellationToken);
        if (auction == null)
            return Result.Failure<BookmarkItemDto>(AuctionErrors.Auction.NotFound);

        var exists = await _bookmarkRepository.ExistsAsync(
            request.UserId, request.AuctionId, BookmarkType.Watchlist, cancellationToken);
        
        if (exists)
            return Result.Failure<BookmarkItemDto>(AuctionErrors.Bookmark.AlreadyExists);

        var bookmark = new Bookmark
        {
            UserId = request.UserId,
            Username = request.Username,
            AuctionId = request.AuctionId,
            Type = BookmarkType.Watchlist,
            NotifyOnBid = request.NotifyOnBid,
            NotifyOnEnd = request.NotifyOnEnd,
            AddedAt = DateTimeOffset.UtcNow
        };

        await _bookmarkRepository.AddAsync(bookmark, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {Username} added auction {AuctionId} to watchlist", 
            request.Username, request.AuctionId);

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

        return Result.Success(result);
    }
}

