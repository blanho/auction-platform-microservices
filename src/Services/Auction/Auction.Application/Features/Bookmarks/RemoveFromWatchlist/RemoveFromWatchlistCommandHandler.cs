using Auctions.Application.Errors;
using Auctions.Domain.Entities;
using Microsoft.Extensions.Logging;
// using BuildingBlocks.Infrastructure.Caching; // Use BuildingBlocks.Application.Abstractions instead
// using BuildingBlocks.Infrastructure.Repository; // Use BuildingBlocks.Application.Abstractions instead

namespace Auctions.Application.Commands.Bookmarks.RemoveFromWatchlist;

public class RemoveFromWatchlistCommandHandler : ICommandHandler<RemoveFromWatchlistCommand>
{
    private readonly IBookmarkRepository _bookmarkRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RemoveFromWatchlistCommandHandler> _logger;

    public RemoveFromWatchlistCommandHandler(
        IBookmarkRepository bookmarkRepository,
        IUnitOfWork unitOfWork,
        ILogger<RemoveFromWatchlistCommandHandler> logger)
    {
        _bookmarkRepository = bookmarkRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(RemoveFromWatchlistCommand request, CancellationToken cancellationToken)
    {
        var bookmark = await _bookmarkRepository.GetByUserAndAuctionAsync(
            request.UserId, request.AuctionId, BookmarkType.Watchlist, cancellationToken);
        
        if (bookmark == null)
            return Result.Failure(AuctionErrors.Bookmark.NotFound);

        await _bookmarkRepository.DeleteAsync(bookmark.Id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {Username} removed auction {AuctionId} from watchlist", 
            request.Username, request.AuctionId);

        return Result.Success();
    }
}

