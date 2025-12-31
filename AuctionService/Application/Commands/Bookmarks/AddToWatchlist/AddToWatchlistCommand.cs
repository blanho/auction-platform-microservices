using AuctionService.Application.DTOs;
using Common.CQRS.Abstractions;

namespace AuctionService.Application.Commands.Bookmarks.AddToWatchlist;

public record AddToWatchlistCommand(
    Guid AuctionId,
    Guid UserId,
    string Username,
    bool NotifyOnBid = true,
    bool NotifyOnEnd = true
) : ICommand<BookmarkItemDto>;
