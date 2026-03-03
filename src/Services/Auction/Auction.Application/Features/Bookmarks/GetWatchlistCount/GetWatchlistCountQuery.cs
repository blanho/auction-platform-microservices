namespace Auctions.Application.Features.Bookmarks.GetWatchlistCount;

public record GetWatchlistCountQuery(
    Guid UserId
) : IQuery<int>;
