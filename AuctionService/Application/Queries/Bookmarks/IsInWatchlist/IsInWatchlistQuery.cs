using Common.CQRS.Abstractions;

namespace AuctionService.Application.Queries.Bookmarks.IsInWatchlist;

public record IsInWatchlistQuery(
    Guid UserId,
    Guid AuctionId
) : IQuery<bool>;
