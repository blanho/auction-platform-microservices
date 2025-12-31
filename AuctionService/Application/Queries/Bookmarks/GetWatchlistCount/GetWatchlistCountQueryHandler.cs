using AuctionService.Application.Interfaces;
using AuctionService.Domain.Entities;
using Common.Core.Helpers;
using Common.CQRS.Abstractions;

namespace AuctionService.Application.Queries.Bookmarks.GetWatchlistCount;

public class GetWatchlistCountQueryHandler : IQueryHandler<GetWatchlistCountQuery, int>
{
    private readonly IUserAuctionBookmarkRepository _bookmarkRepository;

    public GetWatchlistCountQueryHandler(IUserAuctionBookmarkRepository bookmarkRepository)
    {
        _bookmarkRepository = bookmarkRepository;
    }

    public async Task<Result<int>> Handle(GetWatchlistCountQuery request, CancellationToken cancellationToken)
    {
        var count = await _bookmarkRepository.GetCountAsync(
            request.UserId, BookmarkType.Watchlist, cancellationToken);
        return Result.Success(count);
    }
}
