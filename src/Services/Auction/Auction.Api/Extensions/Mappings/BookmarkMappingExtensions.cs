#nullable enable
using Auctions.Application.DTOs;
using Auctions.Application.DTOs.Bookmarks;
using Auctions.Domain.Entities;
using AutoMapper;
using BuildingBlocks.Application.Abstractions;

namespace Auctions.Api.Extensions.Mappings;

public static class BookmarkMappingExtensions
{
    public static BookmarkItemDto ToDto(this Bookmark bookmark, IMapper mapper)
    {
        return mapper.Map<BookmarkItemDto>(bookmark);
    }

    public static List<BookmarkItemDto> ToDtoList(this IEnumerable<Bookmark> bookmarks, IMapper mapper)
    {
        return bookmarks.Select(b => mapper.Map<BookmarkItemDto>(b)).ToList();
    }

    public static PaginatedResult<BookmarkItemDto> ToPaginatedDto(
        this PaginatedResult<Bookmark> result,
        IMapper mapper)
    {
        var dtos = result.Items.ToDtoList(mapper);
        return new PaginatedResult<BookmarkItemDto>(dtos, result.TotalCount, result.Page, result.PageSize);
    }
}
