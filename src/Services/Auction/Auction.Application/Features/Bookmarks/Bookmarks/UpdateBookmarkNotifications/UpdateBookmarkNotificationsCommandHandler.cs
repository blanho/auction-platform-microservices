using Auction.Application.Errors;
using Auctions.Domain.Entities;
using Microsoft.Extensions.Logging;
using BuildingBlocks.Infrastructure.Caching;
using BuildingBlocks.Infrastructure.Repository;

namespace Auctions.Application.Commands.Bookmarks.UpdateBookmarkNotifications;

public class UpdateBookmarkNotificationsCommandHandler : ICommandHandler<UpdateBookmarkNotificationsCommand>
{
    private readonly IUserAuctionBookmarkRepository _bookmarkRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateBookmarkNotificationsCommandHandler(
        IUserAuctionBookmarkRepository bookmarkRepository,
        IUnitOfWork unitOfWork)
    {
        _bookmarkRepository = bookmarkRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateBookmarkNotificationsCommand request, CancellationToken cancellationToken)
    {
        var bookmark = await _bookmarkRepository.GetByUserAndAuctionAsync(
            request.UserId, request.AuctionId, BookmarkType.Watchlist, cancellationToken);
        
        if (bookmark == null)
            return Result.Failure(AuctionErrors.Bookmark.NotFound);

        bookmark.UpdateNotificationSettings(request.NotifyOnBid, request.NotifyOnEnd);

        await _bookmarkRepository.UpdateAsync(bookmark, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

