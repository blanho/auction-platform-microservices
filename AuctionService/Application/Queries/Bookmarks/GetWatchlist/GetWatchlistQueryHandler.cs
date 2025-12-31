using AuctionService.Application.DTOs;
using AuctionService.Application.Interfaces;
using AuctionService.Domain.Entities;
using Common.Core.Helpers;
using Common.CQRS.Abstractions;

namespace AuctionService.Application.Queries.Bookmarks.GetWatchlist;

public class GetWatchlistQueryHandler : IQueryHandler<GetWatchlistQuery, List<BookmarkItemDto>>
{
    private readonly IUserAuctionBookmarkRepository _bookmarkRepository;

    public GetWatchlistQueryHandler(IUserAuctionBookmarkRepository bookmarkRepository)
    {
        _bookmarkRepository = bookmarkRepository;
    }

    public async Task<Result<List<BookmarkItemDto>>> Handle(GetWatchlistQuery request, CancellationToken cancellationToken)
    {
        var items = await _bookmarkRepository.GetByUsernameAsync(
            request.Username, BookmarkType.Watchlist, cancellationToken);

        var result = items.Select(b => new BookmarkItemDto
        {
            Id = b.Id,
            AuctionId = b.AuctionId,
            BookmarkType = b.Type.ToString(),
            AuctionTitle = b.Auction?.Item?.Title ?? string.Empty,
            PrimaryImageFileId = b.Auction?.Item?.Files.FirstOrDefault(f => f.IsPrimary)?.FileId,
            CurrentBid = b.Auction?.CurrentHighBid ?? 0,
            ReservePrice = b.Auction?.ReservePrice ?? 0,
            AuctionEnd = b.Auction?.AuctionEnd ?? DateTimeOffset.MinValue,
            Status = b.Auction?.Status.ToString() ?? string.Empty,
            AddedAt = b.AddedAt,
            NotifyOnBid = b.NotifyOnBid,
            NotifyOnEnd = b.NotifyOnEnd
        }).ToList();

        return Result.Success(result);
    }
}
