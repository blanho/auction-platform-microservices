using AuctionService.Application.Interfaces;
using AuctionService.Domain.Entities;
using Common.Core.Helpers;
using Common.Core.Interfaces;
using Common.CQRS.Abstractions;
using Common.Repository.Interfaces;

namespace AuctionService.Application.Commands.Bookmarks.RemoveFromWatchlist;

public class RemoveFromWatchlistCommandHandler : ICommandHandler<RemoveFromWatchlistCommand>
{
    private readonly IUserAuctionBookmarkRepository _bookmarkRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppLogger<RemoveFromWatchlistCommandHandler> _logger;

    public RemoveFromWatchlistCommandHandler(
        IUserAuctionBookmarkRepository bookmarkRepository,
        IUnitOfWork unitOfWork,
        IAppLogger<RemoveFromWatchlistCommandHandler> logger)
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
            return Result.Failure(Error.Create("Bookmark.NotFound", "Item not found in watchlist"));

        await _bookmarkRepository.DeleteAsync(bookmark.Id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {Username} removed auction {AuctionId} from watchlist", 
            request.Username, request.AuctionId);

        return Result.Success();
    }
}
