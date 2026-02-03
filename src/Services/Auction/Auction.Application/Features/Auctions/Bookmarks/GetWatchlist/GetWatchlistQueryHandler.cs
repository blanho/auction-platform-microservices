using Auctions.Application.DTOs;
using Auctions.Application.Filtering;
using Auctions.Domain.Entities;
using BuildingBlocks.Application.Abstractions;

namespace Auctions.Application.Queries.Bookmarks.GetWatchlist;

public class GetWatchlistQueryHandler : IQueryHandler<GetWatchlistQuery, PaginatedResult<BookmarkItemDto>>
{
    private readonly IBookmarkRepository _bookmarkRepository;

    public GetWatchlistQueryHandler(IBookmarkRepository bookmarkRepository)
    {
        _bookmarkRepository = bookmarkRepository;
    }

    public async Task<Result<PaginatedResult<BookmarkItemDto>>> Handle(GetWatchlistQuery request, CancellationToken cancellationToken)
    {
        var queryParams = new BookmarkQueryParams
        {
            Page = request.Page,
            PageSize = request.PageSize,
            SortBy = request.SortBy,
            SortDescending = request.SortDescending,
            Filter = new BookmarkFilter
            {
                AuctionId = request.AuctionId,
                NotifyOnBid = request.NotifyOnBid,
                NotifyOnEnd = request.NotifyOnEnd,
                FromDate = request.FromDate,
                ToDate = request.ToDate
            }
        };

        var result = await _bookmarkRepository.GetByUsernameAsync(
            request.Username, BookmarkType.Watchlist, queryParams, cancellationToken);

        var items = result.Items.Select(b => new BookmarkItemDto
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

        return Result.Success(new PaginatedResult<BookmarkItemDto>(
            items, result.TotalCount, result.Page, result.PageSize));
    }
}

