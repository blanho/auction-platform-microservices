using AuctionService.Application.Interfaces;
using AuctionService.Domain.Entities;
using Common.Core.Helpers;
using Common.CQRS.Abstractions;
using Common.Repository.Interfaces;

namespace AuctionService.Application.Commands.Bookmarks.UpdateBookmarkNotifications;

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
            return Result.Failure(Error.Create("Bookmark.NotFound", "Item not found in watchlist"));

        bookmark.UpdateNotificationSettings(request.NotifyOnBid, request.NotifyOnEnd);

        await _bookmarkRepository.UpdateAsync(bookmark, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
