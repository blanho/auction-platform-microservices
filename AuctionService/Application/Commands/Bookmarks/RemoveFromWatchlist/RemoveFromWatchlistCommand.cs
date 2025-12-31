using Common.CQRS.Abstractions;

namespace AuctionService.Application.Commands.Bookmarks.RemoveFromWatchlist;

public record RemoveFromWatchlistCommand(
    Guid AuctionId,
    Guid UserId,
    string Username
) : ICommand;
