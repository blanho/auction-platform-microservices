using Auctions.Application.DTOs;
namespace Auctions.Application.Commands.Bookmarks.AddToWatchlist;

public record AddToWatchlistCommand(
    Guid AuctionId,
    Guid UserId,
    string Username,
    bool NotifyOnBid = true,
    bool NotifyOnEnd = true
) : ICommand<BookmarkItemDto>;

