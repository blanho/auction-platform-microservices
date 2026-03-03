namespace Auctions.Application.Features.Bookmarks.IsInWatchlist;

public record IsInWatchlistQuery(
    Guid UserId,
    Guid AuctionId
) : IQuery<bool>;
