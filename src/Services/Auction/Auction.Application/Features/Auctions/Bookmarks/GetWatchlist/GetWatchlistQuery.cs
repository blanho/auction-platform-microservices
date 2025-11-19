using Auctions.Application.DTOs;
namespace Auctions.Application.Queries.Bookmarks.GetWatchlist;

public record GetWatchlistQuery(
    string Username
) : IQuery<List<BookmarkItemDto>>;

