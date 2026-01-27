using Auctions.Application.DTOs;
using BuildingBlocks.Application.CQRS;

namespace Auctions.Application.Features.Bookmarks.GetBookmark;

public record GetBookmarkQuery(Guid AuctionId, Guid UserId) : IQuery<BookmarkItemDto?>;
