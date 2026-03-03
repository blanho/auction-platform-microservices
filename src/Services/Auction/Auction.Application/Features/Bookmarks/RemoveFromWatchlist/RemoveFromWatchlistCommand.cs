namespace Auctions.Application.Features.Bookmarks.RemoveFromWatchlist;

public record RemoveFromWatchlistCommand(
    Guid AuctionId,
    Guid UserId,
    string Username
) : ICommand;

