using AuctionService.Application.DTOs;
using Common.CQRS.Abstractions;

namespace AuctionService.Application.Queries.Bookmarks.GetWatchlist;

public record GetWatchlistQuery(
    string Username
) : IQuery<List<BookmarkItemDto>>;
