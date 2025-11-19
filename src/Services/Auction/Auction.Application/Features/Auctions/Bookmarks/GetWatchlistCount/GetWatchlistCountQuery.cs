namespace Auctions.Application.Queries.Bookmarks.GetWatchlistCount;

public record GetWatchlistCountQuery(
    Guid UserId
) : IQuery<int>;

