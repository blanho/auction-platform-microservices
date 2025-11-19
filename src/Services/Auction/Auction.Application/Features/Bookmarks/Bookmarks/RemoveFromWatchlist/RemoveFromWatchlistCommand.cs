namespace Auctions.Application.Commands.Bookmarks.RemoveFromWatchlist;

public record RemoveFromWatchlistCommand(
    Guid AuctionId,
    Guid UserId,
    string Username
) : ICommand;

