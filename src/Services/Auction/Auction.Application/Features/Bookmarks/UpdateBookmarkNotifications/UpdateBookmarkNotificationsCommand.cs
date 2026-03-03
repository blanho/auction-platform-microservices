namespace Auctions.Application.Features.Bookmarks.UpdateBookmarkNotifications;

public record UpdateBookmarkNotificationsCommand(
    Guid AuctionId,
    Guid UserId,
    bool NotifyOnBid,
    bool NotifyOnEnd
) : ICommand;

