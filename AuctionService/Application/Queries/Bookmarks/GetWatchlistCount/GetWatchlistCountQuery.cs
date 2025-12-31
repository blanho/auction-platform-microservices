using Common.CQRS.Abstractions;

namespace AuctionService.Application.Queries.Bookmarks.GetWatchlistCount;

public record GetWatchlistCountQuery(
    Guid UserId
) : IQuery<int>;
