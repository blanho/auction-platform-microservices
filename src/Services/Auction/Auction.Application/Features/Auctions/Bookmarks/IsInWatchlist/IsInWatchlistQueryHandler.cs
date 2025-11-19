using Auctions.Domain.Entities;
namespace Auctions.Application.Queries.Bookmarks.IsInWatchlist;

public class IsInWatchlistQueryHandler : IQueryHandler<IsInWatchlistQuery, bool>
{
    private readonly IUserAuctionBookmarkRepository _bookmarkRepository;

    public IsInWatchlistQueryHandler(IUserAuctionBookmarkRepository bookmarkRepository)
    {
        _bookmarkRepository = bookmarkRepository;
    }

    public async Task<Result<bool>> Handle(IsInWatchlistQuery request, CancellationToken cancellationToken)
    {
        var exists = await _bookmarkRepository.ExistsAsync(
            request.UserId, request.AuctionId, BookmarkType.Watchlist, cancellationToken);
        return Result.Success(exists);
    }
}

