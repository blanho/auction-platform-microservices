using Auctions.Application.DTOs;
using Auctions.Domain.Entities;
using BuildingBlocks.Application.Abstractions.Logging;
using BuildingBlocks.Infrastructure.Caching;
using BuildingBlocks.Infrastructure.Repository;
using BuildingBlocks.Infrastructure.Repository.Specifications;

namespace Auctions.Application.Commands.Bookmarks.AddToWatchlist;

public class AddToWatchlistCommandHandler : ICommandHandler<AddToWatchlistCommand, BookmarkItemDto>
{
    private readonly IUserAuctionBookmarkRepository _bookmarkRepository;
    private readonly IAuctionRepository _auctionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppLogger<AddToWatchlistCommandHandler> _logger;

    public AddToWatchlistCommandHandler(
        IUserAuctionBookmarkRepository bookmarkRepository,
        IAuctionRepository auctionRepository,
        IUnitOfWork unitOfWork,
        IAppLogger<AddToWatchlistCommandHandler> logger)
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
            return Result.Failure<BookmarkItemDto>(Error.Create("Auction.NotFound", "Auction not found"));

        var exists = await _bookmarkRepository.ExistsAsync(
            request.UserId, request.AuctionId, BookmarkType.Watchlist, cancellationToken);
        
        if (exists)
            return Result.Failure<BookmarkItemDto>(Error.Create("Bookmark.AlreadyExists", "Auction is already in your watchlist"));

        var bookmark = new UserAuctionBookmark
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

